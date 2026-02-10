/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Pages/**/*.{razor,html,cshtml}",
    "./Layout/**/*.{razor,html,cshtml}",
    "./wwwroot/**/*.html",
    "../CRM.V3.Web.Client/**/*.{razor,html,cshtml}",
    "../CRM.V3.Web/**/*.{razor,html,cshtml}",
  ],
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        "primary": "#137fec",
        "background-light": "#f6f7f8",
        "background-dark": "#101922",
      },
      fontFamily: {
        "display": ["Public Sans", "sans-serif"]
      },
      borderRadius: {
        "DEFAULT": "0.25rem",
        "lg": "0.5rem",
        "xl": "0.75rem",
        "full": "9999px"
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/container-queries'),
  ],
}
