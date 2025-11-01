## 🚀 Cómo usar Postman para subir imágenes a Cloudinary

### 1️⃣ **Obtener Token JWT de Administrador**

```http
POST http://localhost:5216/api/auth/login
Content-Type: application/json

{
  "email": "admin@gmail.com",
  "password": "contrasenaadmin"
}
```

**Copia el token de la respuesta.**

---

### 2️⃣ **Crear Producto con Archivos (Sube a Cloudinary)**

#### Configuración en Postman:

1. **Método:** `POST`
2. **URL:** `http://localhost:5216/api/admin/create-with-files`
3. **Headers:**
   ```
   Authorization: Bearer TU_TOKEN_JWT_AQUI
   ```

4. **Body:**
   - Selecciona: `form-data` (NO raw, NO JSON)
   - Agrega los siguientes campos:

   | KEY | TYPE | VALUE |
   |-----|------|-------|
   | `title` | Text | `"iPhone 15 Pro"` |
   | `description` | Text | `"Smartphone de última generación con chip A17"` |
   | `price` | Text | `999` |
   | `stock` | Text | `50` |
   | `status` | Text | `"Nuevo"` |
   | `categoryName` | Text | `"Electrónicos"` |
   | `brandName` | Text | `"Apple"` |
   | `images` | File | [Selecciona archivo de imagen] |
   | `images` | File | [Selecciona otro archivo - opcional] |

**IMPORTANTE:** 
- ✅ Puedes agregar múltiples campos `images` (todos con el mismo nombre)
- ✅ Cada uno debe tener **Type = File**
- ✅ Formatos permitidos: `.jpg`, `.jpeg`, `.png`, `.webp`
- ✅ Tamaño máximo: 5 MB por imagen

---

### 3️⃣ **Verificar en Cloudinary**

1. Ve a tu dashboard de Cloudinary: https://console.cloudinary.com/
2. En el menú lateral, selecciona **Media Library**
3. Deberías ver una carpeta: `product/{productId}/images`
4. Dentro encontrarás las imágenes optimizadas automáticamente

---

## Respuesta Esperada

### ✅ **Respuesta Exitosa (201 Created):**

```json
{
  "message": "Producto creado exitosamente con imágenes subidas a Cloudinary",
  "data": "52"
}
```

---

## Verificar que las imágenes se subieron

### Opción 1: Consultar el producto

```http
GET http://localhost:5216/api/costumer/products/52
```

**Respuesta esperada:**
```json
{
  "message": "Producto obtenido exitosamente",
  "data": {
    "id": 52,
    "title": "iPhone 15 Pro",
    "mainImageURL": "https://res.cloudinary.com/domrtv3xv/image/upload/...",
    "imageUrls": [
      "https://res.cloudinary.com/domrtv3xv/image/upload/...",
      "https://res.cloudinary.com/domrtv3xv/image/upload/..."
    ],
    "price": "999",
    "stock": 50,
    ...
  }
}
