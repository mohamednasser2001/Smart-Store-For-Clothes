SELECT COUNT(*) AS DeletedProductsCount
FROM Products
WHERE IsDeleted = 1;