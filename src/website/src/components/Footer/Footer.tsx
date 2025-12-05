import React from "react";
import styles from "./Footer.module.scss";
import logoSvg from "@/assets/logo/open-raise-logo-white.svg";
import { ArrowRight, ExternalLink } from "lucide-react";

interface FooterProps {
  copyrightText?: string;
}

const Footer: React.FC<FooterProps> = ({
  copyrightText = `© ${new Date().getFullYear()} OpenRaise by Ivy. All rights reserved.`,
}) => {
  return (
    <footer className={styles.footer}>
      <div className={styles.container}>
        {/* Ivy Interactive Section */}
        <div className={styles.ivySection}>
          <div className={styles.ivyBadge}>
            <span className={styles.ivyBadgeText}>
              Built by Ivy Interactive
            </span>
          </div>
          <h2 className={styles.ivyTitle}>Built with Ivy framework.</h2>
          <p className={styles.ivyDescription}>
            Open Race was created by the Ivy team to explore how a real
            fundraising process can be built using our own framework. It
            demonstrates Ivy's ability to support pipelines, analytics,
            onboarding flows, and logic, the same building blocks teams need
            for their internal tools and custom applications. Open Race is both
            a useful product and a live example of what Ivy enables.
          </p>
          <div className={styles.ivyCtaGroup}>
            <a
              href="https://ivy.app"
              target="_blank"
              rel="noopener noreferrer"
              className={styles.ivyPrimaryCta}
            >
              Explore Ivy
              <ArrowRight size={18} />
            </a>
            <a
              href="https://github.com/Ivy-Interactive/Ivy-Framework"
              target="_blank"
              rel="noopener noreferrer"
              className={styles.ivySecondaryCta}
            >
              <svg
                width="18"
                height="18"
                viewBox="0 0 24 24"
                fill="currentColor"
              >
                <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
              </svg>
              View on GitHub
            </a>
            <a
              href="https://ivy.app/docs"
              target="_blank"
              rel="noopener noreferrer"
              className={styles.ivySecondaryCta}
            >
              <ExternalLink size={16} />
              Documentation
            </a>
          </div>
        </div>

        {/* OpenRaise Section */}
        <div className={styles.topSection}>
          <div className={styles.brandSection}>
            <img src={logoSvg} alt="OpenRaise Logo" className={styles.logo} />
            <p className={styles.tagline}>
              Connecting founders with top-tier venture capital firms to
              accelerate growth.
            </p>
          </div>
        </div>

        <div className={styles.bottomSection}>
          <div className={styles.leftSection}>
            <p className={styles.copyright}>{copyrightText}</p>
            <div className={styles.repoLinks}>
              <a
                href="https://github.com/Ivy-Interactive/Ivy-Open-Raise"
                className={styles.repoLink}
                target="_blank"
                rel="noopener noreferrer"
              >
                <svg
                  width="16"
                  height="16"
                  viewBox="0 0 24 24"
                  fill="currentColor"
                >
                  <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
                </svg>
                GitHub
              </a>
              <span className={styles.separator}>•</span>
              <a
                href="https://ivy.app"
                className={styles.repoLink}
                target="_blank"
                rel="noopener noreferrer"
              >
                Built with Ivy
              </a>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
