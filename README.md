# Local Secure Vault (LSV)

## Descripción

**Local Secure Vault** es una aplicación local para Windows destinada a la gestión segura de credenciales y secretos (usuarios, contraseñas, URLs, notas, etc.).

El objetivo principal del proyecto es sustituir el uso de ficheros en texto plano (por ejemplo Markdown) por una solución **segura, cifrada y offline**, que permita realizar copias de seguridad y restaurarlas incluso en otros equipos.

No existe ningún servidor, backend ni servicio cloud:  
todo se ejecuta **exclusivamente en el equipo local del usuario**.

---

## Objetivos principales

- Almacenar toda la información **siempre cifrada en reposo**
- Acceso protegido mediante **una master password** (nunca almacenada)
- Diseño moderno basado en buenas prácticas de seguridad:
  - Derivación de clave con Argon2id
  - Cifrado simétrico con AES-GCM
  - Protección de integridad y autenticación de datos
- Un único fichero cifrado fácil de respaldar
- Restauración posible en otros equipos
- Aplicación 100% local para Windows
- Interfaz moderna basada en **Blazor Hybrid**

---

## Qué NO es este proyecto

- No es un gestor de contraseñas cloud
- No sincroniza datos
- No pretende competir con Bitwarden, 1Password, etc.
- No protege contra malware activo en el sistema

Es una **bóveda personal local**, segura y controlada por el usuario.

---

## Funcionalidad (MVP)

- Crear una bóveda nueva (fichero cifrado)
- Abrir y desbloquear una bóveda existente
- CRUD de entradas:
  - Nombre
  - Usuario
  - Password
  - URL
  - Notas
  - Tags
- Búsqueda rápida
- Copiar contraseña al portapapeles con auto-borrado
- Auto-bloqueo tras inactividad

---

## Tecnologías principales

- .NET (>= 8)
- Blazor Hybrid (MAUI)
- Argon2id (derivación de claves)
- AES-GCM (cifrado autenticado)
- JSON (payload interno, MVP)

---

## Estado del proyecto

🚧 En fase de **diseño y arquitectura**  
No se ha escrito código todavía.

La documentación de arquitectura y el plan de implementación se encuentran en `arch.md`.
