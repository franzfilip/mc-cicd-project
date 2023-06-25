CREATE DATABASE prod;
CREATE DATABASE test;

\c prod

CREATE TABLE products
(
    id BIGSERIAL,
    name TEXT NOT NULL,
    price NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    CONSTRAINT products_pkey PRIMARY KEY (id)
);

INSERT INTO products (name, price) VALUES ('The Agile Aardvark', 14.99);
INSERT INTO products (name, price) VALUES ('The Art of Debugging', 19.99);
INSERT INTO products (name, price) VALUES ('Clean Code: A Programmer''s Brainwash', 29.99);
INSERT INTO products (name, price) VALUES ('Don''t Make Me Think: A User''s Guide to Haiku', 24.99);
INSERT INTO products (name, price) VALUES ('JavaScript for C++ Programmers', 17.99);
INSERT INTO products (name, price) VALUES ('Python for Zombies', 12.99);
INSERT INTO products (name, price) VALUES ('SQL by the Light of the Silvery Moon', 14.99);
INSERT INTO products (name, price) VALUES ('The Joy of Hex', 9.99);
INSERT INTO products (name, price) VALUES ('Head First Object-Oriented Design', 29.99);
INSERT INTO products (name, price) VALUES ('The Zen of Python: Beautiful is Better Than Ugly', 19.99);
INSERT INTO products (name, price) VALUES ('Code Complete: A Practical Handbook of Software Construction', 34.99);
INSERT INTO products (name, price) VALUES ('The Clean Coder: A Code of Conduct for Professional Programmers', 27.99);
INSERT INTO products (name, price) VALUES ('Test-Driven Development: By Example and Error Messages', 22.99);
INSERT INTO products (name, price) VALUES ('The Pragmatic Programmer: From Journeyman to Mastermind', 29.99);
INSERT INTO products (name, price) VALUES ('Refactoring: Improving the Design of Existing Code', 21.99);
INSERT INTO products (name, price) VALUES ('The Mythical Man-Month: Essays on Software Engineering', 25.99);
INSERT INTO products (name, price) VALUES ('Code Simplicity: The Science of Software Design', 16.99);
INSERT INTO products (name, price) VALUES ('The Clean Architecture: A Craftsman''s Guide to Software Structure and Design', 32.99);
INSERT INTO products (name, price) VALUES ('Programming Pearls: Confessions of a Coder', 18.99);
INSERT INTO products (name, price) VALUES ('Design Patterns: Elements of Reusable Object-Oriented Software', 23.99);

CREATE TABLE warehouses
(
    id BIGSERIAL,
    zip NUMERIC NOT NULL,
    CONSTRAINT warehouses_pkey PRIMARY KEY (id)
);

INSERT INTO warehouses (id, zip) VALUES (1, 1010);
INSERT INTO warehouses (id, zip) VALUES (2, 1020);
INSERT INTO warehouses (id, zip) VALUES (3, 1030);
INSERT INTO warehouses (id, zip) VALUES (4, 1040);
INSERT INTO warehouses (id, zip) VALUES (5, 1050);