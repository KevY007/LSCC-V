const mysql = require("mysql2/promise");

module.exports = mysql.createPool({
  host: "194.163.144.228",
  port: "3369",
  user: "dbmandelivery",
  password: "LLW5HUcpJQZ7g45PmR5rHovh7YUb89",
  database: "racenchase",
  waitForConnections: true,
  connectionLimit: 10,
  queueLimit: 0,
});
