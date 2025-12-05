import React from "react";
import styles from "./Hero.module.scss";
import { ArrowRight, Github } from "lucide-react";
import dashboardImg from "@/assets/images/open-raise.png";
import { useGitHubRelease } from "@/hooks/useGitHubRelease";

interface HeroProps {
  title?: string;
  description?: string;
  githubUrl?: string;
}

const Hero: React.FC<HeroProps> = ({
  title = "Fundraising CRM you can reshape, not rent.",
  description = "A simple, open-source system for structuring investor conversations, managing your pipeline, and understanding which parts of your pitch actually create interest without licenses, plugins, or lock-in.",
  githubUrl = "https://github.com/Ivy-Interactive/Ivy-Open-Raise",
}) => {
  const { release, loading } = useGitHubRelease(
    "Ivy-Interactive/Ivy-Open-Raise"
  );

  return (
    <section className={styles.hero}>
      <div className={styles.container}>
        <div className={styles.content}>
          {!loading && release && (
            <a
              href={release.html_url}
              target="_blank"
              rel="noopener noreferrer"
              className={styles.badge}
            >
              <span className={styles.badgeLabel}>{release.tag_name}</span>
              <span className={styles.badgeText}>View release notes</span>
            </a>
          )}
          {!loading && !release && (
            <a
              href={githubUrl}
              target="_blank"
              rel="noopener noreferrer"
              className={styles.badge}
            >
              <span className={styles.badgeLabel}>Latest</span>
              <span className={styles.badgeText}>View on GitHub</span>
            </a>
          )}

          <h1 className={styles.title}>
            {title.split("\n").map((line, i) => (
              <React.Fragment key={i}>
                {line}
                {i < title.split("\n").length - 1 && <br />}
              </React.Fragment>
            ))}
          </h1>

          <p className={styles.description}>{description}</p>

          <div className={styles.ctaGroup}>
            <a
              href={githubUrl}
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
        </div>

        <div className={styles.imageSection}>
          <div className={styles.imageGlow} />
          <div className={styles.imageFrame}>
            <img
              src={dashboardImg}
              alt="OpenRaise Dashboard"
              className={styles.dashboardImage}
            />
          </div>
        </div>
      </div>
    </section>
  );
};

export default Hero;
