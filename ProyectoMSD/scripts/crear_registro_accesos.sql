-- =============================================================
-- Script para crear la tabla registro_accesos en MySQL
-- Ejecutar en tu base de datos mysmartdevicedb
-- =============================================================

-- 1. Crear la tabla de historial de migraciones si no existe
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Registrar las migraciones anteriores como ya aplicadas
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250921145542_Numero1', '9.0.0'),
       ('20250921145815_Numero2', '9.0.0'),
       ('20250921162214_Numer3', '9.0.0');

-- 3. Crear la tabla de registro de accesos
CREATE TABLE IF NOT EXISTS `registro_accesos` (
    `ID` int(11) NOT NULL AUTO_INCREMENT,
    `ID_Usuarios` int(11) NOT NULL,
    `FechaAcceso` datetime NOT NULL,
    `TipoAccion` varchar(50) NOT NULL,
    `Direccion_Ip` varchar(45) DEFAULT NULL,
    `Navegador` varchar(500) DEFAULT NULL,
    `Pagina_Visitada` varchar(250) DEFAULT NULL,
    `Duracion_Sesion` int(11) DEFAULT NULL,
    PRIMARY KEY (`ID`),
    KEY `ID_Usuarios` (`ID_Usuarios`),
    KEY `IX_FechaAcceso` (`FechaAcceso`),
    CONSTRAINT `registro_accesos_ibfk_1` FOREIGN KEY (`ID_Usuarios`) 
        REFERENCES `usuarios` (`ID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- 4. Registrar esta migración como aplicada
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260226223000_AgregarRegistroAccesos', '9.0.0');

SELECT 'Tabla registro_accesos creada exitosamente!' AS Resultado;
