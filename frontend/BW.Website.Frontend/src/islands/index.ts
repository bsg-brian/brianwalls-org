import { createApp } from "vue";

type IslandModule = { default: any };

function parseProps(node: HTMLElement): any {
    const raw = node.dataset.vueProps;
    if (!raw) return {};
    try {
        return JSON.parse(raw);
    } catch (err) {
        console.warn("Invalid JSON in data-vue-props:", raw, err);
        return {};
    }
}

// Finds all feature islands:
//   src/features/**/islands/*.vue
// Each island becomes a lazy-loaded chunk.
const islandModules = import.meta.glob<IslandModule>("../features/**/islands/*.vue");

function toKebabCase(value: string): string {
    return value
        .replace(/([a-z0-9])([A-Z])/g, "$1-$2")
        .replace(/_/g, "-")
        .toLowerCase();
}

function getFeatureAndFile(path: string): { feature: string; fileBase: string } {
    const parts = path.split("/");

    const featureIndex = parts.findIndex(p => p === "features");
    const feature =
        (featureIndex >= 0 ? parts[featureIndex + 1] : undefined) ?? "unknown-feature";

    const lastSegment = parts.length > 0 ? parts[parts.length - 1] : undefined;
    if (!lastSegment) {
        return { feature, fileBase: "unknown" };
    }

    const fileBase = lastSegment.endsWith(".vue")
        ? lastSegment.slice(0, -4)
        : lastSegment;

    return { feature, fileBase };
}



function buildIslandKey(path: string): string {
    const { feature, fileBase } = getFeatureAndFile(path);

    const featureKey = toKebabCase(feature);

    // If the file is the same as the feature (common case), keep it simple:
    // SampleWorkOrders/islands/SampleWorkOrders.vue -> sample-work-orders
    if (fileBase.toLowerCase() === feature.toLowerCase()) {
        return featureKey;
    }

    // Otherwise allow multiple islands per feature:
    // SampleWorkOrders/islands/Index.vue -> sample-work-orders/index
    return `${featureKey}/${toKebabCase(fileBase)}`;
}

export async function mountVueIslands() {
    // Build key -> loader map
    const registry = new Map<string, () => Promise<IslandModule>>();
    for (const [path, loader] of Object.entries(islandModules)) {
        registry.set(buildIslandKey(path), loader as any);
    }

    const nodes = document.querySelectorAll<HTMLElement>("[data-vue-island]");

    for (const node of nodes) {
        const name = node.dataset.vueIsland;
        if (!name) continue;

        const loader = registry.get(name);
        if (!loader) {
            console.warn(
                `No Vue island found for '${name}'. Expected a component under src/features/**/islands/.`
            );
            continue;
        }

        const mod = await loader(); // lazy-load chunk
        const component = mod.default;
        const props = parseProps(node);

        createApp(component, props).mount(node);
    }
}
