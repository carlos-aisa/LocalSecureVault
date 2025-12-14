# Local Secure Vault — Documentation

Este directorio contiene la documentación técnica del proyecto **Local Secure Vault**.

La documentación está pensada para:
- Poder seguir el desarrollo paso a paso
- Entender las decisiones de seguridad sin conocimientos previos en criptografía
- Servir como referencia futura del diseño

---

## Documentos

### Arquitectura y planificación
- [`arch.md`](arch.md)  
  Arquitectura general, principios de diseño y roadmap de implementación.

### Flujos y componentes
- [`flows.md`](flows.md)  
  Diagrama de componentes y flujos principales:
  - Create Vault
  - Unlock Vault
  - Save Changes

### Formato del fichero
- [`file-format.md`](file-format.md)  
  Especificación exacta del formato del fichero cifrado:
  - Header
  - Payload
  - Cifrado y autenticación
  - Versionado

### Seguridad y tests
- [`security-tests.md`](security-tests.md)  
  Lista de tests de seguridad obligatorios para el MVP:
  - KDF (Argon2id)
  - AEAD (AES-GCM)
  - Integridad del fichero
  - Clipboard y auto-lock

---

## Cómo usar esta documentación durante el desarrollo

- Antes de implementar una parte crítica, revisa el documento correspondiente.
- Cada fase del roadmap en `arch.md` debe completarse **con tests**.
- Si algo no está claro en la documentación, se mejora antes de escribir código.

Este proyecto prioriza **entender lo que se construye**, no solo que funcione.
