<template>
    <div>
        <button @click="toggleTheme">Chenge theme</button>
    </div>
</template>

<script>
    export default {
        methods: {
            toggleTheme() {
                //const currentTheme = document.documentElement.getAttribute('data-theme');
                //document.documentElement.setAttribute('data-theme', currentTheme === 'dark' ? 'blue' : 'dark');
            }
        }
    }
</script>