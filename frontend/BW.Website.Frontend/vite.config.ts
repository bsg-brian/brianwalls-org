import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import tailwindcss from "@tailwindcss/vite";
import { resolve } from "path";

export default defineConfig(({ mode }) => {
    const isDebug = mode === "debug";

    return {
        plugins: [
            vue(),
            tailwindcss(),
        ],
        resolve: {
            alias: {
                "@": resolve(__dirname, "./src"),
            },
        },
        build: {
            // Output to the MVC project's wwwroot/dist
            outDir: resolve(__dirname, "../../src/BW.Website.WebUI/wwwroot/dist"),
            emptyOutDir: true,
            minify: isDebug ? false : true,
            sourcemap: isDebug,

            rollupOptions: {
                input: resolve(__dirname, "src/main.ts"),
                output: {
                    entryFileNames: "main.js",

                    // Hash chunks for cache busting (main.js references these)
                    chunkFileNames: isDebug
                        ? "chunks/[name].js"
                        : "chunks/[name].[hash].js",

                    assetFileNames: (assetInfo) => {
                        if (assetInfo.name?.endsWith(".css")) {
                            return "assets/style.css";
                        }
                        if (/\.(png|jpe?g|gif|svg|ico|webp)$/.test(assetInfo.name ?? "")) {
                            return "assets/[name][extname]";
                        }
                        return "assets/[name][extname]";
                    },
                },
            },
        },
    };
});
