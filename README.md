# Tienda UCN API

The objective of this project is to implement a REST API using **ASP.NET Core 8** and **SQLite** to create an ecommerce platform called **Tienda UCN**.  
It includes user authentication with JWT, role-based access control, and product management with Cloudinary image upload integration.

The **Repository Pattern** and **Clean Architecture** principles are applied to ensure maintainability, scalability, and separation of concerns.

Cloudinary is used as an external media storage solution, allowing efficient and secure handling of product images.

---

## Features

- **JWT Authentication** with roles (`Admin`, `Customer`).
- **Admin Product Management (CRUD)** — endpoints protected by role.
- **Cloudinary Integration** — upload, validate, and store product images.
- **Seeder** — creates roles, users, categories, brands, and sample products.
- **Hangfire (Partial Implementation)** — job setup for removing unconfirmed users (pending filtering by `VerificationCode`).
- **Data Validation** using DTOs and ModelState.

---

## Technologies Used

- ASP .Net Core 9
- Entity Framework Core + SQLite
- Cloudinary SDK
- SkiaSharp (image validation)
- Serilog (structured logging)
- Hangfire (background jobs)
- Bogus (data seeding)

---

## Installation

### Prerequisites
Make sure you have the following installed:
- **Visual Studio Code 1.89.1+**
- **ASP .Net Core 9**
- **git 2.45.1+**
- **Postman** (for API testing)

---

### Quick Start

Clone this repository:
```bash
git clone https://github.com/Micro-Roja-E01/Tienda.git
cd Tienda

Open the project in Visual Studio Code:
code .

Restore dependencies:
dotnet restore

Run the application:
dotnet run
```


### Configuration

Before running, copy the content of appsettings.example.json into a new file appsettings.json, and replace the following credentials:

### Required configuration:
| Key                                 | Description                                                                                   |
| ----------------------------------- | --------------------------------------------------------------------------------------------- |
| **JWTSecret**                       | Secure secret key (≥ 32 characters) for token generation                                      |
| **ResendAPIKey**                    | API key for email sending (if enabled)                                                        |
| **Cloudinary**                      | Cloud credentials (`CloudName`, `ApiKey`, `ApiSecret`)                                        |
| **User:AdminUser**                  | Admin user details (Rut: `XXXXXXXX-X`, BirthDate: `YYYY-MM-DD`, PhoneNumber: `+569 XXXXXXXX`) |
| **User:RandomUserPassword**         | Default password for seeded users                                                             |
| **Products:DefaultImageUrl**        | Default image if a product has no image                                                       |
| **Products:ImageAllowedExtensions** | Allowed extensions (e.g., `.jpg`, `.jpeg`, `.png`)                                            |
| **Products:ImageMaxSizeInBytes**    | Maximum file size (e.g., 5242880 = 5 MB)                                                      |
| **Hangfire**                        | Optional configuration for background jobs                                                    |
| **AllowedUserNameCharacters**       | Allowed characters for usernames                                                              |

---

## Postman Testing

You can test the API using the exported Postman collection file `TiendaUCN.postman_collection.json`, along with its test environment `Tienda-UCN.postman_environment.json`.

---

##  Integrantes

| Nombre completo | Rut | Carrera | Correo institucional |
|------------------|--------------------|----------|----------------------|
| Matías Soto Carvajal | 21708975-1 | Ingeniería Civil en Computación e Informática | matias.soto@alumnos.ucn.cl |
| Sebastián Gálvez Vilchez | 21834204-3 | Ingeniería Civil en Computación e Informática | sebastian.galvez@alumnos.ucn.cl |
| Joaquín Dublas Henriquez | 21715440-5 | Ingeniería Civil en Computación e Informática | joaquin.dublas@alumnos.ucn.cl |

---

📅 **Universidad Católica del Norte — 2025**  
Proyecto: *Tienda UCN API*
