CREATE TABLE Address
(
  addressID INT NOT NULL IDENTITY(1,1),
  street nvarchar(50) NOT NULL,
  suite nvarchar(50) NOT NULL,
  city nvarchar(50) NOT NULL,
  zipcode nvarchar(50) NOT NULL,
  lat NUMERIC(18,10) NOT NULL,
  lng NUMERIC(18,10) NOT NULL,
  PRIMARY KEY (addressID)
);

CREATE TABLE Company
(
  companyID INT NOT NULL IDENTITY(1,1),
  name nvarchar(50) NOT NULL,
  catchPhrase nvarchar(50) NOT NULL,
  bs nvarchar(50) NOT NULL,
  PRIMARY KEY (companyID)
);

CREATE TABLE Klijenti
(
  ID INT NOT NULL IDENTITY(1,1),
  ime nvarchar(50) NOT NULL,
  prezime nvarchar(50) NOT NULL,
  email nvarchar(50) NOT NULL,
  username nvarchar(50),
  phone nvarchar(50),
  website nvarchar(50),
  addressID INT,
  companyID INT,
  PRIMARY KEY (ID),
  FOREIGN KEY (addressID) REFERENCES Address(addressID),
  FOREIGN KEY (companyID) REFERENCES Company(companyID)
);