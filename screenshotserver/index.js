const express = require("express");
const app = express();
const cors = require("cors");
const port = 8237;
const middleware = require("./middleware");
const {
  getAllScreenshots,
  getScreenshotByName,
  deleteScreenshotByName,
} = require("./functions");
const multer = require("multer");
const chalk = require("chalk");

const storage = multer.diskStorage({
  destination: function (req, file, cb) {
    cb(null, "screenshots/");
  },
  filename: function (req, file, cb) {
    cb(null, file.originalname);
  },
});
const upload = multer({
  storage: storage,
  fileFilter: (req, file, cb) => {
    if (
      file.mimetype == "image/png" ||
      file.mimetype == "image/jpg" ||
      file.mimetype == "image/jpeg"
    ) {
      cb(null, true);
    } else {
      cb(null, false);
      return cb("Only .png, .jpg and .jpeg format allowed!");
    }
  },
});

app.use(cors());
//app.use("/ss", middleware({ admin: 1 }));

app.use(middleware({ admin: 1 }));

app.post("/ss/upload", upload.single("file"), async (req, res) => {
  let player = req.file.originalname.split(";")[0];
  //let admin = req.file.originalname.split(";adm_").pop().split("+")[0];

  console.log(
    chalk.green(
      `${res.locals.account.Username} [IP: ${res.locals.ip} | Admin: ${
        res.locals.account.Admin
      }] took screenshot of ${chalk.bold(player)}'s game.`
    )
  );
  res.send({ status: 1, message: "File uploaded successfully" });
});

app.get("/ss/list", async (req, res) => {
  res.send(await getAllScreenshots());
});

app.delete("/ss/:screenshot", async (req, res) => {
  const screenshot = await getScreenshotByName(req.params.screenshot);
  if (!screenshot.length)
    return res.status(500).send({ message: "Screenshot not found" });

  if (
    screenshot[0].admin !== res.locals.account.Username &&
    res.locals.account.Admin < 6
  )
    return res.status(500).send({ message: "Insufficient permissions" });

  if (!(await deleteScreenshotByName(screenshot[0].name)))
    return res.status(500).send({ message: "Error while deleting screenshot" });

  console.log(
    chalk.red(
      `${res.locals.account.Username} [IP: ${res.locals.ip} | Admin: ${
        res.locals.account.Admin
      }] deleted ${chalk.bold(screenshot[0].name)}`
    )
  );

  res.send({ status: 1, message: "Screenshot deleted" });
});

app.listen(port, () => {
  console.log(`list app listening on port https://localhost:${port}`);
});
