using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Assessment_M6.Application.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Generete Access Token (JWT)
    public string GenerateAccessToken(int userId, string email, string role)
    {
        var jwtSettings = _configuration.GetSection("Jwt");//obtiene la configuracion
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));//obtiene la key
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);//escoge que algoritmo se va a usar

        //se genera la info que va a tener el token se usa email porque es mas seguro
        //si en un futuro el email se cambia el token se invalida
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        //se guarda el tiempo de expiracion en una variable
        var accessTokenExpirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");
        
        //Se genera el token
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    //Generate RefreshToken
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64]; //se guarda un espacio en memoria de 64bits
        using var rng = RandomNumberGenerator.Create(); //se generan numeros aleatorios
        rng.GetBytes(randomNumber);// se llenan los 54 bits con numeros aleatorios
        return Convert.ToBase64String(randomNumber);
    }

    //Get time token expiration
    public int GetRefreshTokenExpirationDays()
    {
        //se obtiene la configuracion de las variables de entorno
        var jwtSettings = _configuration.GetSection("Jwt");
        //se intenta parcear los dias de expiracion y si no se puede usa 7
        return int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7");
    }

    //Validate expired token
    //esta funcion resive un token que haya expirado en un string valida que la inforamacion del token
    //coincida con la del refresh y si es asi genera un nuevo acces token
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        //trae la informacion de key issuer y audience de las variables de entorno
        var jwtSettings = _configuration.GetSection("Jwt");
        
        //convierte la key a bites
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

        //crea un validador de tokens. esto lee tokens valida tokens y crea tokens
        var tokenHandler = new JwtSecurityTokenHandler();
        
        //valida el formato del estring para que sea qrq.qerwr.qwerq JWT
        if (!tokenHandler.CanReadToken(token))
            throw new SecurityTokenException("Token con formato inválido");
        
        //convierte el string en un objeto JWT para acceder al header, claims(datos del usuario)
        //signature(firma o sello de autenticidad) y a la fecha de expiracion
        var jwtToken = tokenHandler.ReadJwtToken(token);

        //verifica que la firma del token es la mia con el algoritmo HmacSha256
        if (jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256)
            throw new SecurityTokenException("Algoritmo inválido");
        
        //verifica si el token ya expiro
        if (jwtToken.ValidTo > DateTime.UtcNow)
            throw new SecurityTokenException("El token aún no ha expirado");
        
        //se define como se va a validar el token expirado
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };

        //onjeto que representa al usuariao extraido del token
        ClaimsPrincipal principal;

        //valida que el acces token que expiro sea valido
        try
        {
            principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
        }
        catch
        {
            throw new SecurityTokenException("Token inválido");
        }

        return principal;
    }

}