# Arquitectura y Plan de Implementación

Este documento define la arquitectura técnica del proyecto **Local Secure Vault**, así como las decisiones de diseño y las etapas de implementación.

Está pensado para poder seguir el proyecto paso a paso, incluso sin experiencia previa en criptografía aplicada.

---

## 1. Principios de diseño

### 1.1 Seguridad primero

- Todos los datos se almacenan cifrados
- La master password nunca se guarda
- La integridad de los datos debe poder verificarse
- El fichero debe ser inútil para un atacante sin la contraseña

### 1.2 Portabilidad

- La bóveda debe poder restaurarse en otro equipo
- No se depende de claves ligadas al sistema por defecto (DPAPI opcional)

### 1.3 Separación de responsabilidades

- La UI nunca toca criptografía directamente
- El cifrado y almacenamiento están aislados
- El dominio es independiente de infraestructura

---

## 2. Arquitectura general

El proyecto sigue una arquitectura en capas inspirada en Clean Architecture.

### Proyectos

LocalSecureVault.sln
│
|-- Vault.Domain
│ |- Entidades y reglas de negocio
│
|-- Vault.Application
│ |- Casos de uso y lógica de aplicación
│
|-- Vault.Crypto
│ |- Criptografía (Argon2id + AES-GCM)
│
|-- Vault.Storage
│ |- Formato del fichero y acceso a disco
│
|-- Vault.Ui
│ |- Blazor Hybrid (MAUI)

---

## 3. Flujo de seguridad (visión conceptual)

### 3.1 Creación de una bóveda

1. El usuario introduce una master password
2. Se genera un `salt` aleatorio
3. Se derivan claves usando **Argon2id**
4. El contenido inicial (vacío) se serializa a JSON
5. Se cifra el payload con **AES-GCM**
6. Se escribe un único fichero cifrado

---

### 3.2 Apertura de una bóveda

1. Se lee el header del fichero
2. Se derivan claves usando los parámetros almacenados
3. Se descifra el payload
4. Si falla → contraseña incorrecta o fichero corrupto

---

## 4. Criptografía (explicado sin jerga)

### 4.1 Argon2id (derivación de claves)

- Convierte una contraseña humana en una clave fuerte
- Es lento a propósito (≈800 ms)
- Usa mucha memoria para resistir ataques con GPU
- Los parámetros se guardan en el fichero

**Motivo:** proteger contra ataques offline si alguien roba el fichero.

---

### 4.2 AES-GCM (cifrado autenticado)

- Cifra los datos
- Detecta si alguien ha modificado el fichero
- Evita ataques de manipulación silenciosa

**Resultado:** confidencialidad + integridad en un solo paso.

---

## 5. Formato del fichero cifrado (alto nivel)

[ HEADER | CIPHERTEXT | AUTH TAG ]


### Header (no secreto, pero protegido)

Incluye:

- Identificador del fichero
- Versión
- Parámetros de Argon2id
- Salt
- Nonce de cifrado
- Tipo de payload (JSON)
- Flags futuros

El header se usa como **AAD** (Authenticated Associated Data),
por lo que cualquier modificación invalida el descifrado.

---

## 6. Payload interno

Formato JSON (MVP):

- Metadatos de la bóveda
- Lista de entradas:
  - Id
  - Nombre
  - Usuario
  - Password
  - URL
  - Notas
  - Tags
  - Fechas

---

## 7. Auto-lock y memoria

- La bóveda se bloquea tras X minutos de inactividad
- Las claves viven en memoria solo mientras está desbloqueada
- Se borran buffers sensibles cuando es posible (best effort)

---

## 8. Roadmap de implementación

### Fase 0 – Diseño (actual)

- [x] Decisiones de seguridad
- [x] Arquitectura definida
- [x] Documentación inicial

---

### Fase 1 – Dominio y casos de uso

- [x] Entidades del dominio
- [x] Validaciones
- [x] CRUD de entradas
- [x] Búsqueda
- [x] Tests de negocio

---

### Fase 2 – Criptografía

- [x] Implementación Argon2id
- [x] Implementación AES-GCM
- [x] Tests de cifrado/descifrado
- [x] Tests de corrupción

---

### Fase 3 – Almacenamiento

- [x] Formato del fichero
- [x] Escritura atómica
- [x] Locks de fichero
- [x] Tests de persistencia

---

### Fase 4 – Servicios sensibles

- [ ] Clipboard con auto-borrado
- [ ] Auto-lock por inactividad

---

### Fase 5 – UI (Blazor Hybrid)

- [ ] Pantalla de unlock
- [ ] Lista y búsqueda
- [ ] Edición de entradas
- [ ] Copiar contraseña
- [ ] Gestión de bloqueo

---

## 9. Filosofía de desarrollo

- Nada de “copiar código crypto de StackOverflow”
- Todo lo crítico se entiende antes de implementarse
- El proyecto sirve tanto como herramienta real como aprendizaje sólido
