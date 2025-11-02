namespace Tienda.src.Application.DTO.UserDTO
{
    /// <summary>
    /// DTO que representa el perfil del usuario autenticado.
    /// Se utiliza para retornar los datos visibles del usuario.
    /// </summary>
    public class UserProfileDTO
    {
        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public required string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        public required string LastName { get; set; }

        /// <summary>
        /// Género del usuario.
        /// </summary>
        public required string Gender { get; set; }

        /// <summary>
        /// Fecha de nacimiento del usuario.
        /// </summary>
        public required DateTime BirthDate { get; set; }

        /// <summary>
        /// RUT del usuario.
        /// </summary>
        public required string Rut { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// Fecha de registro en la plataforma.
        /// </summary>
        public DateTime RegisteredAt { get; set; }

        /// <summary>
        /// Fecha de última actualización del perfil.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}