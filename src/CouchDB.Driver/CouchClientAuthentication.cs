﻿using CouchDB.Driver.DTOs;
using CouchDB.Driver.Exceptions;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Options;

namespace CouchDB.Driver
{
    public partial class CouchClient
    {
        protected virtual async Task OnBeforeCallAsync(HttpCall httpCall)
        {
            Check.NotNull(httpCall, nameof(httpCall));

            // If session requests no authorization needed
            if (httpCall.Request?.RequestUri?.ToString()?.Contains("_session", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                return;
            }
            switch (_options.AuthenticationType)
            {
                case AuthenticationType.None:
                    break;
                case AuthenticationType.Basic:
                    httpCall.FlurlRequest = httpCall.FlurlRequest.WithBasicAuth(_options.Username, _options.Password);
                    break;
                case AuthenticationType.Cookie:
                    var isTokenExpired =
                        !_cookieCreationDate.HasValue ||
                        _cookieCreationDate.Value.AddMinutes(_options.CookiesDuration) < DateTime.Now;
                    if (isTokenExpired)
                    {
                        await LoginAsync().ConfigureAwait(false);
                    }
                    httpCall.FlurlRequest = httpCall.FlurlRequest.EnableCookies().WithCookie("AuthSession", _cookieToken);
                    break;
                case AuthenticationType.Proxy:
                    httpCall.FlurlRequest = httpCall.FlurlRequest.WithHeader("X-Auth-CouchDB-UserName", _options.Username)
                        .WithHeader("X-Auth-CouchDB-Roles", string.Join(",", _options.Roles));
                    if (_options.Password != null)
                    {
                        httpCall.FlurlRequest = httpCall.FlurlRequest.WithHeader("X-Auth-CouchDB-Token", _options.Password);
                    }
                    break;
                case AuthenticationType.Jwt:
                    if (_options.JwtTokenGenerator == null)
                    {
                        throw new InvalidOperationException("JWT generation cannot be null.");
                    }
                    var jwt = await _options.JwtTokenGenerator().ConfigureAwait(false);
                    httpCall.FlurlRequest = httpCall.FlurlRequest.WithHeader("Authorization", jwt);
                    break;
                default:
                    throw new NotSupportedException($"Authentication of type {_options.AuthenticationType} is not supported.");
            }
        }

        private async Task LoginAsync()
        {
            HttpResponseMessage response = await _flurlClient.Request(Endpoint)
                .AppendPathSegment("_session")
                .PostJsonAsync(new
                {
                    name = _options.Username,
                    password = _options.Password
                })
                .ConfigureAwait(false);

            _cookieCreationDate = DateTime.Now;

            if (!response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> values))
            {
                throw new InvalidOperationException("Error while trying to log-in.");
            }

            var dirtyToken = values.First();
            var regex = new Regex(@"^AuthSession=(.+); Version=1; .*Path=\/; HttpOnly$");
            Match match = regex.Match(dirtyToken);
            if (!match.Success)
            {
                throw new InvalidOperationException("Error while trying to log-in.");
            }

            _cookieToken = match.Groups[1].Value;
        }

        private async Task LogoutAsync()
        {
            OperationResult result = await _flurlClient.Request(Endpoint)
                .AppendPathSegment("_session")
                .DeleteAsync()
                .ReceiveJson<OperationResult>()
                .ConfigureAwait(false);

            if (!result.Ok)
            {
                throw new CouchDeleteException();
            }
        }
    }
}
