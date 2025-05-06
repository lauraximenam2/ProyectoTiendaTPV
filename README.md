# ProyectoTiendaTPV - TPV de Maquillaje

![{1ED4336F-B632-459E-9E63-2EFC71843F45}](https://github.com/user-attachments/assets/32976d7f-708d-41da-abe0-8e8d69a7db4f)

Aplicación de escritorio tipo Terminal Punto de Venta (TPV) desarrollada como ejemplo práctico, orientada a una tienda de maquillaje. Permite gestionar productos, categorías, realizar ventas y visualizar historial.

## Descripción

Este proyecto es una implementación de un sistema TPV básico utilizando tecnologías modernas de .NET y siguiendo buenas prácticas de desarrollo como el patrón MVVM. Sirve como demostración y base para aplicaciones de punto deventa más complejas.

## Funcionalidades Implementadas

### Ventas (TPV)
*   **Catálogo Visual:** Muestra productos disponibles con imágenes, nombre y precio en un formato de botones fácil de usar.
*   **Filtrado y Búsqueda:**
    *   Filtro por Familia y Subfamilia mediante botones seleccionables.
    *   Búsqueda por texto en Nombre, Descripción y Código de Barras del producto.
    *   Botón "Mostrar Todos" para limpiar filtros de categoría.
*   **Control de Stock:**
    *   Los botones de producto se deshabilitan visualmente si el stock es <= 0.
    *   Impide añadir productos sin stock suficiente al ticket.
    *   Impide incrementar la cantidad en el ticket por encima del stock disponible.
*   **Gestión de Ticket Actual:**
    *   Añadir productos al ticket haciendo clic en su botón.
    *   Incrementar/Decrementar cantidad de cada línea del ticket.
    *   Eliminar líneas individuales del ticket.
    *   Visualización clara del Producto, Cantidad y Subtotal por línea.
*   **Cálculo de Total:** Muestra el importe total del ticket actual en tiempo real.
*   **Finalizar Venta:**
    *   Pide confirmación.
    *   Guarda el `Ticket` y sus `TicketItems` en la base de datos.
    *   **Actualiza (descuenta) el stock** de los productos vendidos.
    *   Muestra un "recibo" simple en un cuadro de diálogo.
    *   Limpia el ticket actual para la siguiente venta.
*   **Cancelar Venta:** Pide confirmación y limpia el ticket actual sin guardar.

### Administración
Interfaz organizada en pestañas para la gestión de datos maestros:
*   **Productos:**
    *   Visualización en `DataGrid` con Imagen, Nombre, Precio (€), Cód. Barras, Stock, Subfamilia y Familia.
    *   **CRUD completo:** Añadir, Editar (con carga de datos existentes y previsualización/cambio de imagen), Eliminar (con confirmación y verificación de dependencias en tickets).
    *   **Gestión de Imágenes:** Selección de imagen local, copia a una carpeta `Images` dentro de la aplicación, guardado del nombre de archivo en la BD.
    *   Botón para recargar la lista.
*   **Familias:**
    *   Visualización en `DataGrid`.
    *   **CRUD completo:** Añadir, Editar, Eliminar (con confirmación y verificación de dependencias en subfamilias).
    *   Botón para recargar la lista.
*   **Subfamilias:**
    *   Visualización en `DataGrid` mostrando Familia padre.
    *   **CRUD completo:** Añadir, Editar (con selección de Familia padre), Eliminar (con confirmación y verificación de dependencias en productos).
    *   Botón para recargar la lista.
*   **Historial Ventas:**
    *   Visualización de Tickets guardados (Número, Fecha, Total) ordenados por fecha.
    *   Al seleccionar un ticket, se muestran sus líneas de detalle (Producto, Cantidad, Precio Unitario, Subtotal) en otra tabla.
    *   Botón para recargar la lista.

## Tecnologías y Arquitectura

*   **Plataforma:** .NET 8 
*   **UI:** WPF (Windows Presentation Foundation) con XAML
*   **Acceso a Datos:** Entity Framework Core 8 
*   **Base de Datos:** SQL Server (LocalDB)
*   **Lenguaje:** C#
*   **Arquitectura:** MVVM (Model-View-ViewModel)
    *   **Model:** Clases (`Product`, `Family`, etc.) + `AppDbContext` (EF Core).
    *   **View:** Archivos XAML (`MainWindow`, `SalesView`, `ProductManagementView`, etc.). Uso de `Binding`, `Command`, `DataTemplate`, `ValueConverter`.
    *   **ViewModel:** Clases que exponen datos y lógica a la Vista (`MainViewModel`, `SalesViewModel`, `ProductManagementViewModel`, etc.). Uso intensivo de `CommunityToolkit.Mvvm` (`ObservableObject`, `RelayCommand`).
*   **Librerías Clave:**
    *   `CommunityToolkit.Mvvm`: Para implementación MVVM simplificada.
    *   `Microsoft.EntityFrameworkCore.SqlServer`: Proveedor EF Core para SQL Server.
    *   `Microsoft.EntityFrameworkCore.Tools`: Para migraciones EF Core.
    *   `BCrypt.Net-Next`: Para hashing de contraseñas.

## Estructura del Proyecto

ProyectoTiendaTPV
|-- Converters/ # Clases IValueConverter, IMultiValueConverter
| |-- EqualityConverter.cs
| |-- ImagePathToBitmapConverter.cs
| |-- StockToBooleanConverter.cs
| |-- TicketItemSubtotalConverter.cs 

|-- Data/ # Clases relacionadas con EF Core
| |-- AppDbContext.cs

|-- Enums/ # Enumeraciones
| |-- UserRole.cs

|-- Images/ # (En directorio de salida bin/Debug) Carpeta donde se copian las imágenes
|-- Migrations/ # Archivos generados por EF Core Migrations

|-- Models/ # Clases de entidad (POCOs)
| |-- Family.cs
| |-- Product.cs
| |-- Subfamily.cs
| |-- Ticket.cs
| |-- TicketItem.cs
| |-- TicketLineItem.cs (Modelo específico para la UI de ventas)
| |-- User.cs

|-- ViewModels/ # Lógica de presentación y estado de la UI
| |-- AddEditFamilyViewModel.cs
| |-- AddEditProductViewModel.cs
| |-- AddEditSubfamilyViewModel.cs
| |-- LoginViewModel.cs
| |-- MainViewModel.cs
| |-- ProductManagementViewModel.cs
| |-- SalesViewModel.cs

|-- Views/ # Archivos XAML
| |-- AddEditFamilyWindow.xaml (.cs)
| |-- AddEditProductWindow.xaml (.cs)
| |-- AddEditSubfamilyWindow.xaml (.cs)
| |-- LoginView.xaml (.cs)
| |-- MainWindow.xaml (.cs)
| |-- ProductManagementView.xaml (.cs)
| |-- SalesView.xaml (.cs)

|-- App.xaml (.cs) # Punto de entrada y recursos globales
|-- ProyectoTiendaTPV.csproj # Archivo de proyecto
|-- README.md # Este archivo

## Cómo Empezar

1.  **Clonar el Repositorio:** 
2.  **Abrir con Visual Studio:** Abre el archivo `.sln` con Visual Studio 2022 o superior 
3.  **Configurar Base de Datos:**
    *   Asegúrate de tener una instancia de SQL Server (LocalDB).
    *   Verifica/ajusta la cadena de conexión en `Data/AppDbContext.cs` (método `OnConfiguring`). Por defecto usa LocalDB y crea `ProyectoTiendaTPV_DB`.
    *   Abre la **Consola del Administrador de Paquetes** (Herramientas > Admin. Paquetes NuGet > Consola...).
    *   Ejecuta `Update-Database` para crear la base de datos y aplicar las migraciones. Esto también debería insertar los usuarios de ejemplo (`admin`/`admin123`, `vendedor`/`admin123`)
    *   *(Opcional)* Ejecuta el script SQL de datos de ejemplo (`add_product.sql` - *Deberías crear este archivo y pegar el script que he adjuntado en el repo*) para poblar Familias, Subfamilias y Productos.
4.  **Ejecutar la Aplicación:** Presiona F5 o el botón de inicio en Visual Studio.
    
## Usuarios de acceso
Administrador (`admin`/`admin123`
Vendedor (`vendedor`/`admin123`)

## Posibles Mejoras Futuras

*   Informes de Ventas.
*   Gestión de Stock Avanzada.
*   Manejo de Métodos de Pago.
*   Impresión de Tickets.
*   Gestión de Usuarios (Admin).
*   Mejoras UI/UX Táctil.


