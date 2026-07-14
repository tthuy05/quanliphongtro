import { copyFile, mkdir } from "node:fs/promises";

await mkdir("wwwroot/vendor", { recursive: true });
await Promise.all([
  copyFile("node_modules/alpinejs/dist/cdn.min.js", "wwwroot/vendor/alpine.min.js"),
  copyFile("node_modules/@alpinejs/focus/dist/cdn.min.js", "wwwroot/vendor/alpine-focus.min.js"),
  copyFile("node_modules/chart.js/dist/chart.umd.js", "wwwroot/vendor/chart.umd.js"),
  copyFile("node_modules/lucide/dist/umd/lucide.js", "wwwroot/vendor/lucide.js")
]);
