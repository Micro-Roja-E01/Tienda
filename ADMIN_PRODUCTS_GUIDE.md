# Guía Completa de Gestión de Productos - Admin

### **Uso en Postman:**

```json
POST /api/admin/create-with-files
{
  "title": "iPhone 15 Pro",
  "price": 1200,
  "discount": 15,  // 15% de descuento
  ...
}

// Respuesta incluirá:
{
  "price": 1200,
  "discount": 15,
  "finalPrice": 1020  // Calculado automáticamente
}
```

---

## CRUD Completo

### **1. Crear Producto (POST)**

#### **Opción A: Con Archivos de Imagen (Cloudinary)**

```http
POST /api/admin/create-with-files
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Body (form-data):**

| Key | Type | Value | Requerido |
|-----|------|-------|-----------|
| `title` | Text | "Samsung Galaxy S24" | ✅ |
| `description` | Text | "Smartphone premium..." | ✅ |
| `price` | Text | 1200 | ✅ |
| `stock` | Text | 50 | ✅ |
| `discount` | Text | 10 | ❌ (default: 0) |
| `status` | Text | "Nuevo" | ✅ |
| `categoryName` | Text | "Smartphones" | ✅ |
| `brandName` | Text | "Samsung" | ✅ |
| `images` | **File** | [Seleccionar archivo] | ✅ |
| `images` | **File** | [Seleccionar archivo] | ❌ |

**Validaciones:**
- ✅ Título: 3-50 caracteres, sin HTML
- ✅ Descripción: 10-100 caracteres, sin HTML
- ✅ Precio: > 0
- ✅ Stock: ≥ 0
- ✅ Descuento: 0-100
- ✅ Status: "Nuevo" o "Usado"
- ✅ Imágenes: .jpg/.jpeg/.png/.webp, máx 5MB cada una

**Respuesta (201 Created):**

```json
{
  "message": "Producto creado exitosamente con imágenes subidas a Cloudinary",
  "data": "52"
}
```

**Características:**
- ✅ Sube imágenes a Cloudinary
- ✅ Guarda URLs y PublicIds en BD
- ✅ **Rollback automático** si falla alguna imagen
- ✅ Valida tamaño, extensión y MIME type

---

#### **Opción B: Con URLs de Imágenes (JSON)**

```http
POST /api/admin/create
Authorization: Bearer {token}
Content-Type: application/json
```

**Body (JSON):**

```json
{
  "title": "iPhone 15 Pro Max",
  "description": "El smartphone más avanzado de Apple",
  "price": 1500,
  "stock": 30,
  "discount": 5,
  "status": "Nuevo",
  "categoryName": "Smartphones",
  "brandName": "Apple",
  "imageUrls": [
    {
      "url": "https://example.com/image1.jpg",
      "alt": "Vista frontal"
    },
    {
      "url": "https://example.com/image2.jpg",
      "alt": "Vista trasera"
    }
  ]
}
```

**Respuesta (201 Created):**

```json
{
  "message": "Producto creado exitosamente",
  "data": "53"
}
```

---

### **2. Leer/Listar Productos (GET)**

#### **A. Listar Todos (Admin)**

```http
GET /api/admin/products
Authorization: Bearer {token}
```

**Query Parameters:**

| Parámetro | Tipo | Ejemplo | Descripción |
|-----------|------|---------|-------------|
| `pageNumber` | int | 1 | Número de página |
| `pageSize` | int | 10 | Elementos por página |
| `searchTerm` | string | "iPhone" | Buscar en título/descripción |
| `category` | string | "Smartphones" | Filtrar por categoría |
| `brand` | string | "Apple" | Filtrar por marca |
| `minPrice` | int | 500 | Precio mínimo |
| `maxPrice` | int | 2000 | Precio máximo |
| `status` | string | "Nuevo" | "Nuevo" o "Usado" |
| `hasDiscount` | bool | true | Con descuento aplicado |
| `lowStock` | bool | true | Stock bajo |
| `sortBy` | string | "price" | "price", "title", "createdAt" |
| `sortDir` | string | "asc" | "asc" o "desc" |

**Ejemplo:**

```http
GET /api/admin/products?pageNumber=1&pageSize=20&category=Smartphones&minPrice=800&sortBy=price&sortDir=asc
```

**Respuesta (200 OK):**

```json
{
  "products": [
    {
      "id": 52,
      "title": "iPhone 15 Pro",
      "categoryName": "Smartphones",
      "brandName": "Apple",
      "price": 1200,
      "discount": 10,
      "finalPrice": 1080,
      "stock": 25,
      "isAvailable": true,
      "mainImageURL": "https://res.cloudinary.com/..."
    }
  ],
  "totalCount": 45,
  "totalPages": 3,
  "currentPage": 1,
  "pageSize": 20
}
```

---

#### **B. Obtener Detalle (Admin)**

**Opción 1: Detalle Básico**

```http
GET /api/admin/{productId}
Authorization: Bearer {token}
```

**Opción 2: Detalle Completo con Auditoría**

```http
GET /api/admin/{productId}/detailed
Authorization: Bearer {token}
```

**Respuesta (200 OK) - Detalle Completo:**

```json
{
  "message": "Detalle completo del producto obtenido exitosamente",
  "data": {
    "id": 52,
    "title": "iPhone 15 Pro",
    "description": "Smartphone premium...",
    "price": 1200,
    "discount": 10,
    "finalPrice": 1080,
    "stock": 25,
    "status": "Nuevo",
    "isAvailable": true,
    "isDeleted": false,
    "createdAt": "2025-11-01T10:30:00Z",
    "updatedAt": "2025-11-02T15:45:00Z",
    "deletedAt": null,
    "categoryId": 3,
    "categoryName": "Smartphones",
    "brandId": 5,
    "brandName": "Apple",
    "images": [
      {
        "id": 101,
        "imageUrl": "https://res.cloudinary.com/.../image1.jpg",
        "publicId": "product_52_abc123",
        "createdAt": "2025-11-01T10:30:05Z"
      }
    ],
    "stockIndicator": "En stock"
  }
}
```

---

#### **C. Listar para Clientes (Público)**

```http
GET /api/products
```

**Diferencias con Admin:**
- ❌ No muestra productos con `isAvailable = false`
- ❌ No muestra productos eliminados
- ❌ No incluye información de auditoría

---

### **3. Actualizar Producto (PUT)**

```http
PUT /api/admin/products/{productId}
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

#### **Características:**

- ✅ **Actualización Parcial**: Solo actualiza campos proporcionados
- ✅ **Puede agregar nuevas imágenes**
- ✅ **Puede eliminar imágenes existentes**
- ✅ **Sincroniza Cloudinary y BD**
- ✅ **Valida referencias** (Category, Brand)

#### **Body (form-data) - Todos los campos son OPCIONALES:**

| Key | Type | Value | Descripción |
|-----|------|-------|-------------|
| `title` | Text | "iPhone 15 Pro Max" | Nuevo título |
| `description` | Text | "Actualización..." | Nueva descripción |
| `price` | Text | 1300 | Nuevo precio |
| `stock` | Text | 40 | Nuevo stock |
| `discount` | Text | 15 | Nuevo descuento |
| `status` | Text | "Usado" | Nuevo estado |
| `categoryName` | Text | "Electrónica" | Nueva categoría |
| `brandName` | Text | "Apple" | Nueva marca |
| `newImages` | **File** | [archivo] | Agregar nueva imagen |
| `newImages` | **File** | [archivo] | Agregar otra imagen |
| `imageIdsToDelete[0]` | Text | 101 | ID de imagen a eliminar |
| `imageIdsToDelete[1]` | Text | 102 | ID de otra imagen |

#### **Ejemplos de Uso:**

**Ejemplo 1: Solo actualizar precio y stock**

```http
PUT /api/admin/products/52
Body (form-data):
  price: 1100
  stock: 60
```

**Ejemplo 2: Actualizar y agregar imágenes**

```http
PUT /api/admin/products/52
Body (form-data):
  title: "iPhone 15 Pro - Edición Limitada"
  newImages: [archivo1.jpg]
  newImages: [archivo2.jpg]
```

**Ejemplo 3: Eliminar imágenes antiguas y agregar nuevas**

```http
PUT /api/admin/products/52
Body (form-data):
  description: "Nueva descripción"
  imageIdsToDelete[0]: 101
  imageIdsToDelete[1]: 102
  newImages: [nueva_imagen.jpg]
```

**Respuesta (200 OK):**

```json
{
  "message": "Producto actualizado exitosamente",
  "data": "Producto con ID 52 actualizado exitosamente"
}
```

#### **Proceso Interno:**

1. ✅ Obtiene producto (sin relaciones para evitar conflictos)
2. ✅ Valida que no esté eliminado
3. ✅ Actualiza solo campos proporcionados
4. ✅ Convierte Status string → enum
5. ✅ Valida rango de descuento (0-100)
6. ✅ Crea/obtiene categoría y marca
7. ✅ Elimina imágenes antiguas (Cloudinary + BD)
8. ✅ Sube nuevas imágenes (con validación)
9. ✅ Actualiza `UpdatedAt`
10. ✅ Guarda cambios

---

### **4. Eliminar Producto (DELETE)**

```http
DELETE /api/admin/products/{productId}
Authorization: Bearer {token}
```
**Respuesta (204 No Content):**

```
(Sin body - éxito silencioso)
```

#### **Intentar eliminar de nuevo:**

```http
DELETE /api/admin/products/52
```

**Respuesta (400 Bad Request):**

```json
{
  "message": "Operación no válida",
  "data": "El producto con ID 52 ya ha sido eliminado."
}
```

#### **Verificar Eliminación:**

```http
GET /api/admin/52/detailed
```

**Respuesta:**

```json
{
  "id": 52,
  "isDeleted": true,
  "deletedAt": "2025-11-02T18:30:00Z",
  "isAvailable": false,
  ...
}
```

---
### **Operaciones:**

#### **Subir:**

```http
POST /api/admin/create-with-files
Body:
  images: [archivo1.jpg]
  images: [archivo2.jpg]
```

#### **Eliminar:**

```http
PUT /api/admin/products/52
Body:
  imageIdsToDelete[0]: 101
```

#### **Reemplazar:**

```http
PUT /api/admin/products/52
Body:
  imageIdsToDelete[0]: 101  (elimina vieja)
  newImages: [nueva.jpg]    (agrega nueva)
```

## Filtros Avanzados

### **Filtros Disponibles:**

#### **1. Búsqueda por Texto**

```http
GET /api/admin/products?searchTerm=iPhone
```

Busca en: `Title`, `CategoryName`, `BrandName`

#### **2. Filtro por Categoría**

```http
GET /api/admin/products?category=Smartphones
```

Match exacto (case-insensitive)

#### **3. Filtro por Marca**

```http
GET /api/admin/products?brand=Apple
```

Match exacto (case-insensitive)

#### **4. Rango de Precio**

```http
GET /api/admin/products?minPrice=500&maxPrice=1500
```

Filtra: `Price >= minPrice AND Price <= maxPrice`

#### **5. Filtro por Estado**

```http
GET /api/admin/products?status=Nuevo
```

Opciones: `"Nuevo"` o `"Usado"`

#### **6. Con Descuento**

```http
GET /api/admin/products?hasDiscount=true
```

Filtra: `Discount > 0`

#### **7. Stock Bajo**

```http
GET /api/admin/products?lowStock=true
```

Filtra: `Stock > 0 AND Stock <= fewUnitsThreshold` (configurado en appsettings)

#### **8. Disponibilidad (Admin)**

```http
GET /api/admin/products?isAvailable=false
```

Muestra productos inactivos

#### **9. Ordenamiento**

```http
GET /api/admin/products?sortBy=price&sortDir=asc
```

**sortBy:** `"price"`, `"title"`, `"createdAt"`  
**sortDir:** `"asc"`, `"desc"`

### **Combinaciones:**

#### **Ejemplo 1: Smartphones Apple con descuento, ordenados por precio**

```http
GET /api/admin/products?category=Smartphones&brand=Apple&hasDiscount=true&sortBy=price&sortDir=asc
```

#### **Ejemplo 2: Productos usados con stock bajo**

```http
GET /api/admin/products?status=Usado&lowStock=true
```

#### **Ejemplo 3: Búsqueda en rango de precio**

```http
GET /api/admin/products?searchTerm=Galaxy&minPrice=800&maxPrice=1200
```

---

### **Por Operación:**

#### **POST (Crear):**
- ✅ 201: Producto creado
- ❌ 400: Validación falla (campo requerido, formato inválido)
- ❌ 401/403: No autorizado
- ❌ 500: Error al subir a Cloudinary

#### **GET (Leer):**
- ✅ 200: Datos obtenidos
- ❌ 404: Producto no encontrado
- ❌ 401/403: No autorizado

#### **PUT (Actualizar):**
- ✅ 200: Producto actualizado
- ❌ 400: Datos inválidos, producto eliminado
- ❌ 404: Producto no existe
- ❌ 401/403: No autorizado
- ❌ 500: Error al subir imagen

#### **DELETE (Eliminar):**
- ✅ 204: Producto eliminado
- ❌ 400: Ya eliminado
- ❌ 404: No existe
- ❌ 401/403: No autorizado
- ❌ 500: Error al eliminar de Cloudinary

---

## Ejemplos Completos en Postman

### **Configuración Inicial:**

#### **1. Obtener Token de Admin**

```http
POST http://localhost:5216/api/auth/login
Content-Type: application/json

{
  "email": "matiassotocarvajal@gmail.com",
  "password": "Contrasena1"
}
```

**Guardar token de la respuesta**

#### **2. Configurar Variables de Entorno**

```
baseUrl: http://localhost:5216
adminToken: {token_obtenido}
```

---

### **Escenario 1: Crear Producto Completo**

```http
POST {{baseUrl}}/api/admin/create-with-files
Authorization: Bearer {{adminToken}}
Content-Type: multipart/form-data

Body (form-data):
  title: Samsung Galaxy S24 Ultra
  description: Smartphone premium con cámara de 200MP y S Pen
  price: 1399
  stock: 50
  discount: 10
  status: Nuevo
  categoryName: Smartphones
  brandName: Samsung
  images: [imagen1.jpg]
  images: [imagen2.jpg]
  images: [imagen3.jpg]
```

**Respuesta Esperada:**

```json
Status: 201 Created
{
  "message": "Producto creado exitosamente con imágenes subidas a Cloudinary",
  "data": "55"
}
```

---

### **Escenario 2: Buscar Productos con Filtros**

```http
GET {{baseUrl}}/api/admin/products?category=Smartphones&brand=Samsung&minPrice=1000&maxPrice=1500&hasDiscount=true&sortBy=price&sortDir=asc
Authorization: Bearer {{adminToken}}
```

**Respuesta Esperada:**

```json
Status: 200 OK
{
  "products": [
    {
      "id": 55,
      "title": "Samsung Galaxy S24 Ultra",
      "price": 1399,
      "discount": 10,
      "finalPrice": 1259,
      "stock": 50,
      ...
    }
  ],
  "totalCount": 1,
  "totalPages": 1,
  "currentPage": 1,
  "pageSize": 10
}
```

---

### **Escenario 3: Actualizar Producto**

```http
PUT {{baseUrl}}/api/admin/products/55
Authorization: Bearer {{adminToken}}
Content-Type: multipart/form-data

Body (form-data):
  price: 1299
  stock: 40
  discount: 15
  newImages: [nueva_imagen.jpg]
```

**Respuesta Esperada:**

```json
Status: 200 OK
{
  "message": "Producto actualizado exitosamente",
  "data": "Producto con ID 55 actualizado exitosamente"
}
```

---

### **Escenario 4: Obtener Detalle Completo**

```http
GET {{baseUrl}}/api/admin/55/detailed
Authorization: Bearer {{adminToken}}
```

**Respuesta Esperada:**

```json
Status: 200 OK
{
  "message": "Detalle completo del producto obtenido exitosamente",
  "data": {
    "id": 55,
    "title": "Samsung Galaxy S24 Ultra",
    "price": 1299,
    "discount": 15,
    "finalPrice": 1104,
    "isDeleted": false,
    "createdAt": "2025-11-02T10:00:00Z",
    "updatedAt": "2025-11-02T15:30:00Z",
    "deletedAt": null,
    "images": [
      {
        "id": 201,
        "imageUrl": "https://res.cloudinary.com/.../image1.jpg",
        "publicId": "product_55_abc123",
        "createdAt": "2025-11-02T10:00:05Z"
      }
    ],
    ...
  }
}
```

---

### **Escenario 5: Eliminar Producto**

```http
DELETE {{baseUrl}}/api/admin/products/55
Authorization: Bearer {{adminToken}}
```

**Respuesta Esperada:**

```
Status: 204 No Content
(Sin body)
```

**Verificar Eliminación:**

```http
GET {{baseUrl}}/api/admin/55/detailed
Authorization: Bearer {{adminToken}}
```

**Respuesta:**

```json
Status: 200 OK
{
  "data": {
    "id": 55,
    "isDeleted": true,
    "deletedAt": "2025-11-02T18:45:00Z",
    "isAvailable": false,
    ...
  }
}
```


### **Escenario 6: Restaurar producto**
```http
PATCH http://localhost:5216/api/admin/products/55/restore
Authorization: Bearer {token}
```

**Respuesta Esperada:**

```json
Status: 200 OK

{   
  "message": "Producto restaurado exitosamente",
  "data": "Producto con ID 55 restaurado exitosamente"
}
```

---

### **Escenario 6: Intentar Operación Inválida**

#### **A. Crear con HTML malicioso**

```http
POST {{baseUrl}}/api/admin/create-with-files
Body:
  title: <script>alert('XSS')</script>iPhone
```

**Respuesta:**

```json
Status: 400 Bad Request
{
  "message": "Error de validación",
  "data": "El campo Title contiene contenido HTML no permitido..."
}
```

#### **B. Actualizar producto eliminado**

```http
PUT {{baseUrl}}/api/admin/products/55
Body:
  price: 1000
```

**Respuesta:**

```json
Status: 400 Bad Request
{
  "message": "Operación no válida",
  "data": "No se puede actualizar un producto eliminado."
}
```

#### **C. Eliminar producto ya eliminado**

```http
DELETE {{baseUrl}}/api/admin/products/55
```

**Respuesta:**

```json
Status: 400 Bad Request
{
  "message": "Operación no válida",
  "data": "El producto con ID 55 ya ha sido eliminado."
}
```