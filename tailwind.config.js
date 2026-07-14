/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./Views/**/*.cshtml", "./wwwroot/js/**/*.js"],
  theme: {
    extend: {
      fontFamily: { sans: ["Arial", "system-ui", "sans-serif"] },
      fontSize: { xxs: ["0.625rem", { lineHeight: "0.875rem" }] },
      spacing: { "0.2": "0.05rem", "4.5": "1.125rem", "6.5": "1.625rem", "8.5": "2.125rem" },
      scale: { "97": ".97" },
      minHeight: { auto: "auto" },
      colors: {
        primary: { DEFAULT: "hsl(243, 75%, 59%)", dark: "hsl(243, 75%, 51%)", light: "hsl(243, 75%, 92%)" },
        success: { DEFAULT: "hsl(161, 84%, 39%)", light: "hsl(161, 84%, 95%)" },
        warning: { DEFAULT: "hsl(38, 92%, 50%)", light: "hsl(38, 92%, 95%)" },
        danger: { DEFAULT: "hsl(343, 84%, 50%)", light: "hsl(343, 84%, 95%)" },
        info: { DEFAULT: "hsl(199, 89%, 48%)", light: "hsl(199, 89%, 95%)" }
      }
    }
  },
  plugins: []
};
