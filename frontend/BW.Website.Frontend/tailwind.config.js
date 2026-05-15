// tailwind.config.js
module.exports = {
    content: [
        "./src/**/*.{js,ts,vue,jsx,tsx,css,html}",
        "../../src/BW.Website.WebUI/Views/**/*.{cshtml,razor}"
    ],
    theme: {
        extend: {
            colors: {
                brand: {
                    DEFAULT: "#2563eb",
                    light: "#60a5fa",
                    dark: "#1d4ed8"
                }
            }
        }
    },
    plugins: []
};
