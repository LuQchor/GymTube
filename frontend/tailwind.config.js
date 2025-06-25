module.exports = {
    content: [
      './components/**/*.{vue,js,ts}',
      './layouts/**/*.{vue,js,ts}',
      './pages/**/*.{vue,js,ts}',
      './app.vue',
    ],
    theme: {
      extend: {},
    },
    plugins: [require('daisyui')],
    daisyui: {
      themes: [
        {
          light: {
            ...require("daisyui/src/theming/themes")["light"],
            primary: "#570df8",
            secondary: "#f000b8",
            accent: "#37cdbe",
            neutral: "#3d4451",
            "base-100": "#ffffff",
          },
        },
        {
          dark: {
            ...require("daisyui/src/theming/themes")["dark"],
            primary: "#793ef9",
            secondary: "#f000b8",
            accent: "#37cdbe",
            neutral: "#3d4451",
            "base-100": "#2a303c",
          },
        },
      ],
    },
  }