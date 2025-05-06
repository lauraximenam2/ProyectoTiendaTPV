-- Asegúrate de estar usando la base de datos correcta
USE ProyectoTiendaTPV_DB;
GO

PRINT 'Limpiando datos existentes...';
-- Eliminar en orden inverso a las dependencias para evitar errores de FK:
DELETE FROM dbo.TicketItems;
DELETE FROM dbo.Tickets;
DELETE FROM dbo.Products;
DELETE FROM dbo.Subfamilies;
DELETE FROM dbo.Families;
GO

-- (Opcional) Reiniciar los contadores de ID
PRINT 'Reiniciando contadores de ID...';
DBCC CHECKIDENT ('dbo.TicketItems', RESEED, 0);
DBCC CHECKIDENT ('dbo.Tickets', RESEED, 0);
DBCC CHECKIDENT ('dbo.Products', RESEED, 0);
DBCC CHECKIDENT ('dbo.Subfamilies', RESEED, 0);
DBCC CHECKIDENT ('dbo.Families', RESEED, 0);
GO

-- Insertar Familias (10)
PRINT 'Insertando Familias...';
INSERT INTO Families (Name) VALUES
('Rostro'), ('Ojos'), ('Labios'), ('Uñas'), ('Cuidado Facial'),
('Cuidado Corporal'), ('Brochas y Herramientas'), ('Perfumes'), ('Accesorios'), ('Sets y Kits');
GO

-- Insertar Subfamilias (21 - Añadida Herramientas Varias)
PRINT 'Insertando Subfamilias...';
DECLARE @fRostro INT = (SELECT Id FROM Families WHERE Name = 'Rostro');
DECLARE @fOjos INT = (SELECT Id FROM Families WHERE Name = 'Ojos');
DECLARE @fLabios INT = (SELECT Id FROM Families WHERE Name = 'Labios');
DECLARE @fUnas INT = (SELECT Id FROM Families WHERE Name = 'Uñas');
DECLARE @fCuidadoFacial INT = (SELECT Id FROM Families WHERE Name = 'Cuidado Facial');
DECLARE @fCuidadoCorporal INT = (SELECT Id FROM Families WHERE Name = 'Cuidado Corporal');
DECLARE @fHerramientas INT = (SELECT Id FROM Families WHERE Name = 'Brochas y Herramientas');
DECLARE @fPerfumes INT = (SELECT Id FROM Families WHERE Name = 'Perfumes');
DECLARE @fAccesorios INT = (SELECT Id FROM Families WHERE Name = 'Accesorios');
DECLARE @fSets INT = (SELECT Id FROM Families WHERE Name = 'Sets y Kits');

INSERT INTO Subfamilies (Name, FamilyId) VALUES
('Bases de Maquillaje', @fRostro), ('Correctores', @fRostro), ('Polvos (Compactos/Sueltos)', @fRostro), ('Coloretes', @fRostro), ('Iluminadores/Bronceadores', @fRostro),
('Sombras de Ojos', @fOjos), ('Delineadores (Eyeliner)', @fOjos), ('Máscaras de Pestañas', @fOjos), ('Cejas', @fOjos),
('Barras de Labios', @fLabios), ('Gloss / Brillos', @fLabios), ('Delineadores Labiales', @fLabios),
('Esmaltes de Uñas', @fUnas), ('Tratamientos Uñas', @fUnas),
('Limpiadores Faciales', @fCuidadoFacial), ('Hidratantes Faciales', @fCuidadoFacial), ('Mascarillas Faciales', @fCuidadoFacial),
('Hidratantes Corporales', @fCuidadoCorporal),
('Brochas Rostro', @fHerramientas), ('Brochas Ojos', @fHerramientas),
('Herramientas Varias', @fHerramientas); -- Subfamilia añadida
GO

-- Insertar Productos (50) - Con datos ficticios NO NULL para Barcode, Description, StockQuantity e ImagePath='algodon.jpg'
PRINT 'Insertando Productos...';
DECLARE @sfBases INT = (SELECT Id FROM Subfamilies WHERE Name = 'Bases de Maquillaje');
DECLARE @sfCorrectores INT = (SELECT Id FROM Subfamilies WHERE Name = 'Correctores');
DECLARE @sfPolvos INT = (SELECT Id FROM Subfamilies WHERE Name = 'Polvos (Compactos/Sueltos)');
DECLARE @sfColoretes INT = (SELECT Id FROM Subfamilies WHERE Name = 'Coloretes');
DECLARE @sfIluminadores INT = (SELECT Id FROM Subfamilies WHERE Name = 'Iluminadores/Bronceadores');
DECLARE @sfSombras INT = (SELECT Id FROM Subfamilies WHERE Name = 'Sombras de Ojos');
DECLARE @sfDelineadoresOjos INT = (SELECT Id FROM Subfamilies WHERE Name = 'Delineadores (Eyeliner)');
DECLARE @sfMascaras INT = (SELECT Id FROM Subfamilies WHERE Name = 'Máscaras de Pestañas');
DECLARE @sfCejas INT = (SELECT Id FROM Subfamilies WHERE Name = 'Cejas');
DECLARE @sfBarrasLabios INT = (SELECT Id FROM Subfamilies WHERE Name = 'Barras de Labios');
DECLARE @sfGloss INT = (SELECT Id FROM Subfamilies WHERE Name = 'Gloss / Brillos');
DECLARE @sfDelineadoresLabios INT = (SELECT Id FROM Subfamilies WHERE Name = 'Delineadores Labiales');
DECLARE @sfEsmaltes INT = (SELECT Id FROM Subfamilies WHERE Name = 'Esmaltes de Uñas');
DECLARE @sfTratamientosUnas INT = (SELECT Id FROM Subfamilies WHERE Name = 'Tratamientos Uñas');
DECLARE @sfLimpiadores INT = (SELECT Id FROM Subfamilies WHERE Name = 'Limpiadores Faciales');
DECLARE @sfHidratantesFacial INT = (SELECT Id FROM Subfamilies WHERE Name = 'Hidratantes Faciales');
DECLARE @sfMascarillas INT = (SELECT Id FROM Subfamilies WHERE Name = 'Mascarillas Faciales');
DECLARE @sfHidratantesCorp INT = (SELECT Id FROM Subfamilies WHERE Name = 'Hidratantes Corporales');
DECLARE @sfBrochasRostro INT = (SELECT Id FROM Subfamilies WHERE Name = 'Brochas Rostro');
DECLARE @sfBrochasOjos INT = (SELECT Id FROM Subfamilies WHERE Name = 'Brochas Ojos');
DECLARE @sfHerramientasVarias INT = (SELECT Id FROM Subfamilies WHERE Name = 'Herramientas Varias'); -- Variable declarada

INSERT INTO Products (Name, Description, Price, StockQuantity, Barcode, ImagePath, SubfamilyId) VALUES
-- Rostro
('Base Fluida Larga Duración SPF15 - Tono Beige', 'Cobertura media, acabado natural.', 22.50, 50, '841111100001', 'algodon.jpg', @sfBases),
('Base Compacta Mate - Tono Arena', 'Control de brillos, ideal piel grasa.', 18.90, 40, '841111100002', 'algodon.jpg', @sfBases),
('Corrector Líquido Iluminador - Tono Light', 'Disimula ojeras y realza.', 12.00, 60, '841111100003', 'algodon.jpg', @sfCorrectores),
('Corrector en Barra Alta Cobertura - Tono Medium', 'Cubre imperfecciones.', 9.95, 70, '841111100004', 'algodon.jpg', @sfCorrectores),
('Polvos Translúcidos Sueltos', 'Matifican y sellan el maquillaje.', 15.50, 100, '841111100005', 'algodon.jpg', @sfPolvos),
('Polvos Compactos Bronceadores - Sol Suave', 'Efecto bronceado natural.', 17.80, 45, '841111100006', 'algodon.jpg', @sfIluminadores),
('Colorete en Polvo - Tono Melocotón', 'Color saludable y duradero.', 11.25, 80, '841111100007', 'algodon.jpg', @sfColoretes),
('Colorete en Crema - Tono Rosa Nude', 'Acabado jugoso y fácil de difuminar.', 13.50, 55, '841111100008', 'algodon.jpg', @sfColoretes),
('Iluminador Líquido Dorado', 'Gotas de luz para rostro y cuerpo.', 16.00, 65, '841111100009', 'algodon.jpg', @sfIluminadores),
('Iluminador en Polvo Champagne', 'Brillo intenso y modulable.', 14.75, 75, '841111100010', 'algodon.jpg', @sfIluminadores),
-- Ojos
('Paleta Sombras "Warm Neutrals" (12 tonos)', 'Colores cálidos mates y shimmer.', 28.00, 30, '841111100011', 'algodon.jpg', @sfSombras),
('Sombra Individual Crema - Bronce', 'Larga duración, no necesita prebase.', 8.50, 90, '841111100012', 'algodon.jpg', @sfSombras),
('Sombra Individual Polvo - Azul Noche', 'Pigmentación intensa.', 6.90, 110, '841111100013', 'algodon.jpg', @sfSombras),
('Delineador Líquido Negro Intenso - Punta Fina', 'Precisión y duración waterproof.', 10.50, 150, '841111100014', 'algodon.jpg', @sfDelineadoresOjos),
('Delineador Lápiz Khol Negro', 'Difuminable para efecto ahumado.', 7.20, 200, '841111100015', 'algodon.jpg', @sfDelineadoresOjos),
('Delineador Gel Marrón con Pincel', 'Resistente al agua.', 13.00, 80, '841111100016', 'algodon.jpg', @sfDelineadoresOjos),
('Máscara Pestañas Volumen Extremo - Negro', 'Cepillo grande para máximo volumen.', 12.95, 180, '841111100017', 'algodon.jpg', @sfMascaras),
('Máscara Pestañas Alargadora Waterproof - Negro', 'Define y alarga sin grumos.', 14.50, 160, '841111100018', 'algodon.jpg', @sfMascaras),
('Máscara Pestañas Transparente Fijadora', 'Fija y define look natural.', 9.00, 70, '841111100019', 'algodon.jpg', @sfMascaras),
('Lápiz de Cejas Rellenador - Castaño Medio', 'Define y rellena huecos.', 8.90, 130, '841111100020', 'algodon.jpg', @sfCejas),
('Gel Fijador Cejas Transparente', 'Mantiene las cejas en su sitio.', 7.50, 100, '841111100021', 'algodon.jpg', @sfCejas),
('Kit Cejas Polvo + Cera - Rubio Oscuro', 'Para unas cejas definidas.', 16.80, 60, '841111100022', 'algodon.jpg', @sfCejas),
-- Labios
('Barra Labios Mate "Rojo Pasión"', 'Color intenso y aterciopelado.', 14.00, 90, '841111100023', 'algodon.jpg', @sfBarrasLabios),
('Barra Labios Cremosa "Nude Caramelo"', 'Hidratante y confortable.', 12.50, 110, '841111100024', 'algodon.jpg', @sfBarrasLabios),
('Barra Labios Líquida Fija "Vino Tinto"', 'Larga duración intransferible.', 16.90, 75, '841111100025', 'algodon.jpg', @sfBarrasLabios),
('Brillo Labial Transparente Efecto Volumen', 'Labios jugosos.', 9.90, 150, '841111100026', 'algodon.jpg', @sfGloss),
('Gloss con Color "Rosa Brillante"', 'Color sutil y brillo espejo.', 11.00, 130, '841111100027', 'algodon.jpg', @sfGloss),
('Aceite Labial Hidratante con Color "Cereza"', 'Nutre y da un toque de color.', 10.50, 85, '841111100028', 'algodon.jpg', @sfGloss),
('Delineador Labial "Natural"', 'Define el contorno, evita que se corra.', 6.50, 200, '841111100029', 'algodon.jpg', @sfDelineadoresLabios),
('Delineador Labial "Rojo Clásico"', 'Combina con labiales rojos.', 6.50, 180, '841111100030', 'algodon.jpg', @sfDelineadoresLabios),
-- Uñas
('Esmalte Uñas "Rojo Ferrari"', 'Brillo intenso y secado rápido.', 5.95, 250, '841111100031', 'algodon.jpg', @sfEsmaltes),
('Esmalte Uñas "Nude Elegante"', 'Color discreto y duradero.', 5.95, 300, '841111100032', 'algodon.jpg', @sfEsmaltes),
('Esmalte Uñas "Top Coat Efecto Gel"', 'Aporta brillo y prolonga duración.', 7.00, 150, '841111100033', 'algodon.jpg', @sfEsmaltes),
('Base Fortalecedora Uñas Débiles', 'Tratamiento endurecedor.', 8.50, 100, '841111100034', 'algodon.jpg', @sfTratamientosUnas),
('Aceite Cutículas Hidratante', 'Nutre y suaviza cutículas.', 6.80, 120, '841111100035', 'algodon.jpg', @sfTratamientosUnas),
-- Cuidado Facial
('Agua Micelar Piel Sensible 400ml', 'Limpia y desmaquilla suavemente.', 9.90, 80, '841111100036', 'algodon.jpg', @sfLimpiadores),
('Gel Limpiador Purificante Piel Grasa', 'Controla sebo y limpia poros.', 11.50, 70, '841111100037', 'algodon.jpg', @sfLimpiadores),
('Crema Hidratante Ligera SPF30', 'Hidratación diaria con protección.', 19.90, 100, '841111100038', 'algodon.jpg', @sfHidratantesFacial),
('Sérum Ácido Hialurónico', 'Hidratación intensa y efecto relleno.', 25.50, 60, '841111100039', 'algodon.jpg', @sfHidratantesFacial),
('Mascarilla Tisú Hidratante Granada', 'Efecto flash de hidratación.', 3.50, 200, '841111100040', 'algodon.jpg', @sfMascarillas),
('Mascarilla Arcilla Verde Purificante', 'Limpia poros y matifica.', 14.00, 90, '841111100041', 'algodon.jpg', @sfMascarillas),
-- Cuidado Corporal
('Leche Corporal Hidratante Almendras 500ml', 'Piel suave y nutrida.', 8.90, 120, '841111100042', 'algodon.jpg', @sfHidratantesCorp),
('Manteca Corporal Karité Ultra-rica', 'Para pieles muy secas.', 15.75, 70, '841111100043', 'algodon.jpg', @sfHidratantesCorp),
-- Brochas y Herramientas
('Brocha Polvos Grande Pelo Sintético', 'Aplicación uniforme de polvos.', 14.90, 80, '841111100044', 'algodon.jpg', @sfBrochasRostro),
('Brocha Colorete Biselada', 'Define pómulos.', 11.80, 90, '841111100045', 'algodon.jpg', @sfBrochasRostro),
('Set 5 Brochas Esenciales Ojos', 'Difuminar, aplicar, delinear.', 19.95, 100, '841111100046', 'algodon.jpg', @sfBrochasOjos),
('Brocha Difuminar Sombra Cónica', 'Acabado profesional.', 7.50, 150, '841111100047', 'algodon.jpg', @sfBrochasOjos),
('Esponja Maquillaje Silicona', 'No absorbe producto.', 6.00, 300, '841111100048', 'algodon.jpg', @sfHerramientasVarias), -- Usando la variable correcta
('Pinzas Depilar Cangrejo', 'Agarre firme.', 4.50, 200, '841111100049', 'algodon.jpg', @sfHerramientasVarias), -- Usando la variable correcta
('Rizador Pestañas Térmico', 'Curva duradera con calor.', 15.50, 180, '841111100050', 'algodon.jpg', @sfHerramientasVarias); -- Usando la variable correcta
GO

PRINT 'Datos de ejemplo insertados.';
GO