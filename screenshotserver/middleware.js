const db = require("./db");

module.exports = function (options) {
  return function (req, res, next) {
    if (!req.headers["x-api-key"])
      return res.status(500).send({ message: "Missing API key" });
    const apikey = req.headers["x-api-key"].split("@");
    if (!apikey[0] || !apikey[1])
      return res.status(500).send({ message: "Invalid API key" });

    db.query(
      "SELECT Username,Admin FROM Accounts WHERE Password = ? AND Salt = ? LIMIT 1",
      [apikey[0], apikey[1]]
    )
      .then(([rows, fields]) => {
        if (!rows.length) return res.status(500).send("Invalid API key");
        const ip = req.headers["x-forwarded-for"] || req.socket.remoteAddress;
        console.log(
          `${rows[0].Username} [IP: ${ip} | Admin: ${rows[0].Admin}] tried to access ${req.url}`
        );
        if (rows[0].Admin < options.admin)
          return res.status(500).send({ message: "Insufficient permissions" });

        res.locals.account = rows[0];
        res.locals.ip = ip;
        next();
      })
      .catch(console.log);
  };
};
