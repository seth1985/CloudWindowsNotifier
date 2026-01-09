/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./index.html",
        "./src/**/*.{js,ts,jsx,tsx}",
    ],
    darkMode: 'class',
    theme: {
        extend: {
            colors: {
                background: "var(--bg-page)",
                card: "var(--bg-card)",
                input: "var(--bg-input)",
                nav: "var(--bg-nav)",
                "surface-hover": "var(--bg-hover)",
                "surface-active": "var(--bg-active)",
                "surface-chip": "var(--bg-chip)",
                "surface-chip-active": "var(--bg-chip-active)",
                "nav-text": "var(--text-nav)",

                primary: {
                    DEFAULT: "var(--primary-main)",
                    main: "var(--primary-main)",
                    strong: "var(--primary-strong)",
                    soft: "var(--primary-soft)",
                    foreground: "var(--primary-contrast)",
                },

                secondary: {
                    DEFAULT: "var(--bg-hover)",
                    foreground: "var(--text-primary)",
                },

                border: "var(--border-default)",
                "input-border": "var(--border-input)",
                "border-focus": "var(--border-focus)",

                text: {
                    primary: "var(--text-primary)",
                    secondary: "var(--text-secondary)",
                    tertiary: "var(--text-tertiary)",
                    inverse: "var(--text-inverse)",
                },

                muted: {
                    DEFAULT: "var(--bg-hover)",
                    foreground: "var(--text-secondary)",
                },
            },
            borderRadius: {
                lg: "14px",
                md: "10px",
                sm: "6px",
            },
            boxShadow: {
                card: "var(--shadow-card)",
                modal: "var(--shadow-modal)",
            }
        },
    },
    plugins: [],
}
