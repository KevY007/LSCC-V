const fs = require("fs");
const fsPromises = fs.promises;

async function getAllScreenshots() {
  let files = await fsPromises.readdir("../screenshots");
  files = files.map(async (file) => {
    const stats = await fsPromises.stat(`../screenshots/${file}`);
    return {
      name: file,
      player: file.split(";")[0],
      admin: file.split(";adm_").pop().split("+")[0],
      size: stats.size,
      date: stats.mtime,
    };
  });

  return await Promise.all(files);
}
async function getScreenshotsByAdmin(admin) {
  let screenshots = await getAllScreenshots();
  return screenshots.filter((screenshot) => screenshot.admin === admin);
}
async function getScreenshotsByPlayer(player) {
  let screenshots = await getAllScreenshots();
  return screenshots.filter((screenshot) => screenshot.player === player);
}
async function getScreenshotByName(name) {
  let screenshots = await getAllScreenshots();
  return screenshots.filter((screenshot) => screenshot.name === name);
}
async function deleteScreenshotByName(name) {
  let screenshot = await getScreenshotByName(name);
  if (!screenshot.length) return false;
  await fsPromises.unlink(`../screenshots/${screenshot[0].name}`);
  return true;
}

module.exports = {
  getAllScreenshots,
  getScreenshotsByAdmin,
  getScreenshotsByPlayer,
  getScreenshotByName,
  deleteScreenshotByName,
};
