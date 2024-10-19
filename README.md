# Inventory Management System
**Final Project for Digital Egypt Pioneers Initiative (.NET Web Developer Track)**

# Overview
This Inventory Management System is a comprehensive web-based solution developed using the latest technologies in the .NET ecosystem. The project was created as part of the Digital Egypt Pioneers Initiative (DEPI) final submission for the .NET Web Developer Track. The system is designed to manage products, categories, suppliers, and stock levels efficiently, providing real-time insights into inventory status, with features for reporting and stock value calculations.

# Features
* **User Management**: Role-based access control for Admin and Staff users.
* **Product Management**: CRUD operations for products, categories, and suppliers.
* **Reporting**: Export stock reports in both PDF and Excel formats, with enhanced designs and visual cues for low stock or out-of-stock items.
* **Real-Time Insights**: Dynamic stock status with indicators for low stock levels and out-of-stock items.
* **Responsive Design**: A modern user interface ensuring a seamless experience across devices.

# Technologies Used
* **ASP.NET Core MVC**: For building the web application.
* **Entity Framework Core**: For database management.
* **SQL Server**: For managing data storage.
* **DinkToPdf**: For generating PDF reports.
* **EPPlus**: For generating Excel reports.
* **Bootstrap 5**: For responsive front-end design.

# Installation
To run this project locally, follow these steps:

**Prerequisites**
* .NET 8.0 SDK or later
* SQL Server or any other supported database
* Node.js (optional for front-end assets)
* Git

**Steps**
1. Clone the repository:
     git clone https://github.com/Ahmed0Tawfik/Inventory-Management-System.git
2. Navigate to the project folder:
     cd Inventory-Management-System
3. Install dependencies:
     - In the root directory, open the NuGet Package Manager Console and install the necessary packages:
         Install-Package DinkToPdf
         Install-Package EPPlus
     - The file (libwkhtmltox.dll) must be existed in the following path in the project folder, so that the feature of generating PDF reports can work properly:
         Inventory_Management_System\Inventory\bin\Debug\net8.0
5. Run the following command to apply migrations:
     Update-Database
6. Run the application:
     dotnet run

# Usage
**Admin Features:**
* Create and manage roles and users.
* Manage products, categories, and suppliers.
* Export products from the stock.
* Generate PDF and Excel reports of products and stock levels.
* View detailed stock information, including low stock and out-of-stock warnings and the total stock value.

**Staff Features:**
* View products, categories, and suppliers.
* Export products from the stock.
* Track stock levels and receive low stock and out-of-stock warnings.

# Reporting Features
This system includes advanced reporting features to assist administrators in monitoring inventory:
* **PDF Reports**: Visually enhanced, with color-coded stock warnings and a summary of the total stock value.
* **Excel Reports**: Cleanly formatted with data on product categories, suppliers, prices, and stock quantities. Color-coded for easy identification of stock issues.
