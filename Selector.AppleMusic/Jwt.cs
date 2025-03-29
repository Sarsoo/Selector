// https://github.com/CurtisUpdike/AppleDeveloperToken
// MIT License
//
// Copyright (c) 2023 Curtis Updike
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Selector.Web.Apple;

internal record AppleAccount(string TeamId, string KeyId, string PrivateKey);

public class TokenGenerator
{
    private static readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly AppleAccount _account;
    private int _secondsValid;
    public int SecondsValid
    {
        get { return _secondsValid; }
        set
        {
            ValidateTime(value);
            _secondsValid = value;
        }
    }

    public TokenGenerator(string privateKey, string teamId, string keyId, int secondsValid = 15777000)
    {
        ValidateTime(secondsValid);
        _account = new(teamId, keyId, FormatKey(privateKey));
        _secondsValid = secondsValid;
    }

    public string Generate()
    {
        return GenerateToken(_account, TimeSpan.FromSeconds(SecondsValid));
    }

    public string Generate(int secondsValid)
    {
        ValidateTime(secondsValid);
        return GenerateToken(_account, TimeSpan.FromSeconds(secondsValid));

    }

    public string Generate(TimeSpan timeValid)
    {
        ValidateTime(timeValid.Seconds);
        return GenerateToken(_account, timeValid);
    }

    private static string GenerateToken(AppleAccount account, TimeSpan timeValid)
    {
        var now = DateTime.UtcNow;
        var algorithm = CreateAlgorithm(account.PrivateKey);
        var signingCredentials = CreateSigningCredentials(account.KeyId, algorithm);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = account.TeamId,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.Add(timeValid),
            SigningCredentials = signingCredentials
        };

        var token = _tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    private static ECDsa CreateAlgorithm(string key)
    {
        var algorithm = ECDsa.Create();
        algorithm.ImportPkcs8PrivateKey(Convert.FromBase64String(key), out _);
        return algorithm;
    }

    private static SigningCredentials CreateSigningCredentials(string keyId, ECDsa algorithm)
    {
        var key = new ECDsaSecurityKey(algorithm) { KeyId = keyId };
        return new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256);
    }

    private static void ValidateTime(int seconds)
    {
        if (seconds > 15777000)
        {
            throw new ArgumentException("Must be less than 15777000 seconds (6 months).");
        }
    }

    private static string FormatKey(string key)
    {
        return key.Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "");
    }
}