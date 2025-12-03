import React, { useEffect, useRef } from "react";
import styles from "./CalendlySection.module.scss";

interface CalendlySectionProps {
  url?: string;
  title?: string;
  subtitle?: string;
}

declare global {
  interface Window {
    Calendly?: {
      initInlineWidget: (options: {
        url: string;
        parentElement: HTMLElement;
      }) => void;
    };
  }
}

const CalendlySection: React.FC<CalendlySectionProps> = ({
  url = "https://calendly.com/d/cv34-53t-7q5/ivy-demo-call-with-our-founding-growth-team?hide_gdpr_banner=1&background_color=ffffff&text_color=000000&primary_color=00cc92",
  title = "Book a Demo",
  subtitle = "Schedule a call with our team to see OpenRaise in action",
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const initializedRef = useRef(false);

  useEffect(() => {
    const initWidget = () => {
      if (
        containerRef.current &&
        window.Calendly &&
        !initializedRef.current
      ) {
        containerRef.current.innerHTML = "";
        initializedRef.current = true;
        window.Calendly.initInlineWidget({
          url,
          parentElement: containerRef.current,
        });
      }
    };

    // Load the Calendly script
    const existingScript = document.querySelector(
      'script[src="https://assets.calendly.com/assets/external/widget.js"]'
    );

    if (existingScript) {
      // Script already loaded, init immediately
      if (window.Calendly) {
        initWidget();
      }
    } else {
      const script = document.createElement("script");
      script.src = "https://assets.calendly.com/assets/external/widget.js";
      script.async = true;
      script.onload = () => {
        initWidget();
      };
      document.head.appendChild(script);
    }

    // Reset on unmount so it can reinit on next mount
    return () => {
      initializedRef.current = false;
    };
  }, [url]);

  return (
    <section className={styles.calendlySection} id="calendly">
      <div className={styles.container}>
        <div className={styles.header}>
          <div className={styles.badge}>
            <span className={styles.badgeIcon}>📅</span>
            <span className={styles.badgeText}>Schedule a Call</span>
          </div>
          <h2 className={styles.title}>{title}</h2>
          <p className={styles.subtitle}>{subtitle}</p>
        </div>
        <div className={styles.calendlyWrapper}>
          <div ref={containerRef} className={styles.calendlyEmbed} />
        </div>
      </div>
    </section>
  );
};

export default CalendlySection;
