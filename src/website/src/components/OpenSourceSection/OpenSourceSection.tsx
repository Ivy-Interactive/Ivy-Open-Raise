import React from "react";
import styles from "./OpenSourceSection.module.scss";
import { Database, Infinity, Scale, Github, ArrowRight } from "lucide-react";

const benefits = [
  {
    icon: <Database size={24} />,
    title: "Full control",
    description: "Over your data and infrastructure",
  },
  {
    icon: <Infinity size={24} />,
    title: "No limits",
    description: "Scale without seat pricing",
  },
  {
    icon: <Scale size={24} />,
    title: "MIT License",
    description: "Modify and extend freely",
  },
];

const OpenSourceSection: React.FC = () => {
  return (
    <section className={styles.openSourceSection} id="open-source">
      <div className={styles.container}>
        <div className={styles.badge}>
          <span className={styles.badgeText}>OPEN SOURCE</span>
        </div>

        <h2 className={styles.title}>
          Your fundraising stack should belong to you.
        </h2>

        <p className={styles.description}>
          Open-source gives founders control over data, workflows, and the
          evolution of their tools. No seat limits, no upgrade walls, just
          freedom.
        </p>

        <div className={styles.benefits}>
          {benefits.map((benefit, index) => (
            <div key={index} className={styles.benefit}>
              <div className={styles.iconTitleRow}>
                <span className={styles.icon}>{benefit.icon}</span>
                <h3 className={styles.benefitTitle}>{benefit.title}</h3>
              </div>
              <p className={styles.benefitDescription}>{benefit.description}</p>
            </div>
          ))}
        </div>

        <a
          href="https://github.com/Ivy-Interactive/Ivy-Open-Raise"
          target="_blank"
          rel="noopener noreferrer"
          className={styles.githubCta}
        >
          <Github size={18} />
          View on GitHub
          <ArrowRight size={18} />
        </a>
      </div>
    </section>
  );
};

export default OpenSourceSection;
