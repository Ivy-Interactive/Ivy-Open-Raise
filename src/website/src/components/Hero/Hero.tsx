import React from "react";
import styles from "./Hero.module.scss";
import { ArrowRight, Github, Calendar } from "lucide-react";
import dashboardImg from "@/assets/images/open-raise.png";

interface HeroProps {
  badge?: string;
  badgeLink?: string;
  title?: string;
  description?: string;
  primaryCtaText?: string;
  secondaryCtaText?: string;
  githubUrl?: string;
}

const Hero: React.FC<HeroProps> = ({
  badge = "Explore Our New AI Features",
  badgeLink = "#",
  title = "Smarter Fundraising for\nFaster Startup Growth",
  description = "Track your fundraising performance in real-time, manage investor pipelines effortlessly, and gain insights that help you close deals faster.",
  primaryCtaText = "View on GitHub",
  secondaryCtaText = "Book a Demo",
  githubUrl = "https://github.com/Ivy-Interactive/Ivy-Open-Raise",
}) => {
  const scrollToCalendly = () => {
    const calendlySection = document.getElementById("calendly");
    if (calendlySection) {
      calendlySection.scrollIntoView({ behavior: "smooth" });
    }
  };

  return (
    <section className={styles.hero}>
      <div className={styles.container}>
        <div className={styles.content}>
          <a href={badgeLink} className={styles.badge}>
            <span className={styles.badgeLabel}>News</span>
            <span className={styles.badgeText}>{badge}</span>
          </a>

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
              {primaryCtaText}
              <ArrowRight size={18} />
            </a>
            <button className={styles.secondaryCta} onClick={scrollToCalendly}>
              <Calendar size={16} />
              {secondaryCtaText}
            </button>
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
