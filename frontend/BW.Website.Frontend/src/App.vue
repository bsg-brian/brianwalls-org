<script setup lang="ts">
import HelloWorld from './components/HelloWorld.vue'
</script>

<template>
    
    <div class="min-h-screen flex items-center justify-center bg-slate-900 text-slate-100">
        <div class="p-8 rounded-2xl bg-slate-800/80 shadow-lg">
            <h1 class="text-3xl font-bold mb-2">
                Tailwind v4 + Vite + Vue
            </h1>
            <p class="text-sm text-slate-300">
                If this looks nicely styled, Tailwind 4 is working.
            </p>

            <div>
                <a href="https://vite.dev" target="_blank">
                    <img src="/vite.svg" class="logo" alt="Vite logo" />
                </a>
                <a href="https://vuejs.org/" target="_blank">
                    <img src="./assets/vue.svg" class="logo vue" alt="Vue logo" />
                </a>
            </div>
            <HelloWorld msg="Vite + Vue" />
        </div>
    </div>

</template>

<style scoped>
    .logo {
      height: 6em;
      padding: 1.5em;
      will-change: filter;
      transition: filter 300ms;
    }
    .logo:hover {
      filter: drop-shadow(0 0 2em #646cffaa);
    }
    .logo.vue:hover {
      filter: drop-shadow(0 0 2em #42b883aa);
    }
	a {
		font-weight: 500;
		color: #646cff;
		text-decoration: inherit;
	}

	a:hover {
		color: #535bf2;
	}
    button {
		    border-radius: 8px;
		    border: 1px solid transparent;
		    padding: 0.6em 1.2em;
		    font-size: 1em;
		    font-weight: 500;
		    font-family: inherit;
		    background-color: #1a1a1a;
		    cursor: pointer;
		    transition: border-color 0.25s;
	    }

    button:hover {
		    border-color: #646cff;
	    }

    button:focus,
    button:focus-visible {
		    outline: 4px auto -webkit-focus-ring-color;
	    }
</style>
