create database pr32;
use pr32;
create table Role
(
  RoleID int primary key auto_increment,
  RoleName varchar(50) not null
);
create table PickUpPoint
(
  PickUpPointID int primary key auto_increment,
  PickUpPointName varchar(255) not null
);
create table Suplier
(
  SuplierID int primary key auto_increment,
  SuplierName varchar(100) not null
);
create table CategoryProduct
(
  CategoryProductID int primary key auto_increment,
  CategoryProductrName varchar(50) not null
);
create table User
(
  UserID int primary key auto_increment,
  UserSurname varchar(100) not null,
  UserName varchar(100) not null,
  UserPatronymic varchar(100) not null,
  UserLogin varchar(20) not null,
  UserPassword varchar(20) not null,
  UserRole int not null,
  foreign key (UserRole) references Role(RoleID) 
);
create table Product
(
  ProductArticleNumber varchar(100) primary key,
  ProductName varchar(100) not null,
  ProductUnitMeasurement enum('шт.', 'уп.'),
  ProductCost float not null,
  ProductDiscountMax int null,
  ProductManufacturer varchar(50) not null,
  ProductSuplier int null,
  ProductCategory int null,
  ProductDiscountAmount int null,
  ProductQuantityInStock int not null,
  ProductDescription varchar(255) not null,
  ProductPhoto varchar(30),
  foreign key (ProductSuplier) references Suplier(SuplierID),
  foreign key (ProductCategory) references CategoryProduct(CategoryProductID)
);
create table OrderDetails
(
  OrderDetailsID int primary key auto_increment,
  OrderDetailsDate date not null,
  OrderDetailsDeliveryDate date not null,
  OrderDetailsPickupPoint int not null,
  OrderDetailsUser int not null,
  OrderDetailsCode int not null,
  OrderDetailsStatus enum('Новый', 'Завершен'),
  foreign key (OrderDetailsPickupPoint) references PickUpPoint(PickUpPointID),
  foreign key (OrderDetailsUser) references User(UserID)
);
create table orderr
(
  OrderID int not null,
  OrderProductArticleNumber varchar(100) not null,
  OrderQuantity int not null,
  Primary key (OrderID,OrderProductArticleNumber),
  foreign key (OrderID) references OrderDetails(OrderDetailsID),
  foreign key (OrderProductArticleNumber) references Product(ProductArticleNumber)
);