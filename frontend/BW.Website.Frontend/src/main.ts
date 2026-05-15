//Optional SPA mount
//import { createApp } from "vue";
//import App from "./App.vue";

// Tailwind
import "./css/style.css";

// Islands
import { mountVueIslands } from "./islands";

// Optional SPA mount (only if #app exists)
//const spaRoot = document.getElementById("app");
//if (spaRoot) {
//	createApp(App).mount(spaRoot);
//}

// Island auto-mount (lazy loaded per island)
mountVueIslands();
