import React from "react";
import styles from "./CTASection.module.scss";
import { Github, ArrowRight, ExternalLink } from "lucide-react";

const CTASection: React.FC = () => {
  return (
    <section className={styles.ctaSection}>
      <div className={styles.container}>
        <h2 className={styles.title}>
          Run your raise with clarity and control.
        </h2>

        <div className={styles.ctaButtons}>
          <a
            href="https://github.com/Ivy-Interactive/Ivy-Open-Raise"
            target="_blank"
            rel="noopener noreferrer"
            className={styles.primaryCta}
          >
            <Github size={18} />
            Deploy from GitHub
            <ArrowRight size={18} />
          </a>
          <a href="#" className={styles.secondaryCta}>
            Try hosted version (48h free)
          </a>
        </div>

        <a
          href="https://github.com/Ivy-Interactive/Ivy-Open-Raise"
          target="_blank"
          rel="noopener noreferrer"
          className={styles.repoLink}
        >
          <ExternalLink size={16} />
          View GitHub Repo
        </a>
      </div>
    </section>
  );
};

export default CTASection;
