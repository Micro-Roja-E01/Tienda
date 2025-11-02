using Mapster;
using Tienda.src.Application.Domain.Models;
using Tienda.src.Application.DTO.AuthDTO;

namespace Tienda.src.Application.Mappers
{
    /// <summary>
    /// Clase para mapear objetos de tipo DTO a User y viceversa.
    /// </summary>
    public class UserMapper
    {
        public UserMapper() { }

        /// <summary>
        /// Configura todos los mapeos relacionados con autenticación de usuarios.
        /// </summary>
        public void ConfigureAllMappings()
        {
            ConfigureAuthMappings();
        }

        /// <summary>
        /// Configura el mapeo de <see cref="RegisterDTO"/> hacia <see cref="User"/>.
        /// Define asignación de UserName, Email y datos personales.
        /// </summary>
        public void ConfigureAuthMappings()
        {
            TypeAdapterConfig<RegisterDTO, User>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.Email)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.BirthDate, src => src.BirthDate)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.EmailConfirmed, src => false);
        }
    }
}
