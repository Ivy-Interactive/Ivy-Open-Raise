import React from "react";
import styles from "./HowToStartSection.module.scss";

const steps = [
  {
    number: "1",
    title: "Deploy from GitHub",
    description:
      "Start instantly using your own infrastructure or our hosting partner.",
  },
  {
    number: "2",
    title: "Add investors & deals",
    description: "Start with demo data or import your list.",
  },
  {
    number: "3",
    title: "Share a deck link",
    description: "Track engagement and build real momentum.",
  },
];

const HowToStartSection: React.FC = () => {
  return (
    <section className={styles.howToStartSection} id="get-started">
      <div className={styles.container}>
        <div className={styles.badge}>
          <span className={styles.badgeText}>GET STARTED</span>
        </div>

        <h2 className={styles.title}>How to start</h2>

        <div className={styles.steps}>
          {steps.map((step) => (
            <div key={step.number} className={styles.step}>
              <div className={styles.iconTitleRow}>
                <span className={styles.stepNumber}>{step.number}</span>
                <h3 className={styles.stepTitle}>{step.title}</h3>
              </div>
              <p className={styles.stepDescription}>{step.description}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
};

export default HowToStartSection;
