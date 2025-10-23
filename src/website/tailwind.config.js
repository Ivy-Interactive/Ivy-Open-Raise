/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'primary-dark': '#041209',
        'teal': '#00cc92',
        'gray-light': '#f5f5f5',
        'gray-hover': '#e5e5e5',
        'dark': '#171717',
      },
      fontFamily: {
        'inter': ['Inter', 'sans-serif'],
      },
      letterSpacing: {
        'tighter-sm': '-0.28px',
        'tighter-md': '-0.36px',
        'tighter': '-0.48px',
        'tighter-lg': '-0.96px',
        'tighter-xl': '-1px',
        'tighter-2xl': '-1.12px',
        'tighter-3xl': '-1.6px',
      },
    },
  },
  plugins: [],
}

