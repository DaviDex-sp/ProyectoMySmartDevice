# 🏠 MySmartDevice - Plataforma Domótica Centralizada

## 📖 Descripción del Proyecto
**MySmartDevice** es una aplicación web diseñada para centralizar y simplificar la gestión de hogares inteligentes (domótica). El objetivo principal de la plataforma es ofrecer a los usuarios una interfaz única e intuitiva desde la cual puedan monitorizar y controlar diversos dispositivos de su entorno, como sistemas de iluminación, enchufes inteligentes, electrodomésticos y sensores ambientales.

## 🛠️ Tecnologías Utilizadas
Este proyecto está construido aplicando buenas prácticas de desarrollo y patrones de diseño estructurados, utilizando el siguiente stack tecnológico:
* **Lenguaje:** C#
* **Framework:** ASP.NET Core - Razor Pages
* **ORM:** Entity Framework Core
* **Base de Datos:** MySQL / MariaDB (Soporte para bases de datos relacionales)
* **Frontend:** HTML5, CSS3, JavaScript (Integrado con sintaxis Razor)

## ⚠️ Estado Actual y Roadmap (Fase de Investigación)
Actualmente, la aplicación cuenta con la estructura base del sistema, el panel de administración, la gestión de usuarios y el registro lógico de dispositivos en la base de datos. 

**Nota importante sobre las funcionalidades de control:**
Gran parte de las características *core* de interacción física (el encendido/apagado real de luces o la lectura de métricas en tiempo real) se encuentran actualmente **en desarrollo y planificación**. 

La integración directa con hardware domótico requiere una comunicación precisa y segura. Nos encontramos en una fase activa de **investigación (I+D)** evaluando:
* El funcionamiento y las limitaciones de los diferentes sensores del mercado.
* Los protocolos de conexión y comunicación IoT más eficientes y seguros (como MQTT, WebSockets o integraciones de APIs de terceros como Tuya/Sonoff).
* La latencia y respuesta de los actuadores físicos.

A medida que se definan los estándares de conexión con el hardware, estas funcionalidades se irán desplegando en futuras actualizaciones.

## 🚀 Instalación y Uso Local
1. Clona este repositorio: 
2. Abre la solución en Visual Studio.
3. Restaura los paquetes NuGet necesarios.
4. Configura tu cadena de conexión MySQL en el archivo `appsettings.json` o en tus variables de entorno locales.
5. Ejecuta las migraciones para construir la base de datos: `dotnet ef database update`
6. Inicia el proyecto (IIS Express o Kestrel).
