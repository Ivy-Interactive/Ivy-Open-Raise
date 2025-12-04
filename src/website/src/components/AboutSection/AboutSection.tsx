import React from "react";
import styles from "./AboutSection.module.scss";

interface Stat {
  value: string;
  label: string;
}

interface AboutSectionProps {
  title?: string;
  description?: string;
  stats?: Stat[];
}

const AboutSection: React.FC<AboutSectionProps> = ({
  title = "Our Platform is designed to give teams clear insights, real-time tracking, and powerful tools to close deals faster.",
  description,
  stats = [
    { value: "25M+", label: "Active Users" },
    { value: "$50B+", label: "Revenue" },
    { value: "300%", label: "Faster Sales" },
    { value: "500K+", label: "Deals Closed" },
  ],
}) => {
  return (
    <section className={styles.aboutSection} id="about">
      <div className={styles.container}>
        <div className={styles.badge}>
          <span className={styles.badgeIcon}>ⓘ</span>
          <span className={styles.badgeText}>About Us</span>
        </div>

        <h2 className={styles.title}>{title}</h2>

        {description && <p className={styles.description}>{description}</p>}

        <div className={styles.stats}>
          {stats.map((stat, index) => (
            <div key={index} className={styles.statItem}>
              <span className={styles.statValue}>{stat.value}</span>
              <span className={styles.statLabel}>{stat.label}</span>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
};

export default AboutSection;

