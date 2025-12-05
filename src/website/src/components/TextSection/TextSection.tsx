import React from "react";
import styles from "./TextSection.module.scss";

interface TextSectionProps {
  title?: string;
  subtitle?: string;
  content?: string;
  alignment?: "left" | "center" | "right";
  variant?: "default" | "highlight";
}

const TextSection: React.FC<TextSectionProps> = ({
  title = "Empowering Founders Worldwide",
  subtitle = "Building the future of fundraising",
  content = "Open Raise is revolutionizing how startups connect with investors. Our platform brings together ambitious founders and visionary investors, creating opportunities that drive innovation forward. With our data-driven approach and extensive network, we've helped thousands of startups secure the funding they need to scale their businesses and achieve their goals.",
  alignment = "center",
  variant = "default",
}) => {
  return (
    <section className={`${styles.textSection} ${styles[variant]}`}>
      <div className={styles.container}>
        <div className={`${styles.content} ${styles[alignment]}`}>
          {title && <h2 className={styles.title}>{title}</h2>}
          {subtitle && <p className={styles.subtitle}>{subtitle}</p>}
          {content && (
            <div className={styles.textContent}>
              {content.split("\n").map((paragraph, index) => (
                <p key={index} className={styles.paragraph}>
                  {paragraph}
                </p>
              ))}
            </div>
          )}
        </div>
      </div>
    </section>
  );
};

export default TextSection;