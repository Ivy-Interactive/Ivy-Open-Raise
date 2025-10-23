import React from "react";
import styles from "./Hero.module.scss";
import { Search } from "lucide-react";
import { Button } from "../ui/button";
import logoSvg from "@/assets/logo/open-raise-logo-white.svg";

interface HeroProps {
  title?: string;
  description?: string;
  smallTitle?: string;
  ctaText?: string;
  onCtaClick?: () => void;
}

const Hero: React.FC<HeroProps> = ({
  title = "Hero title",
  description = "Description",
  smallTitle = "Small title",
  ctaText = "Search",
  onCtaClick,
}) => {
  return (
    <div className={styles.hero}>
      <div className={styles.container}>
        <header className={styles.header}>
          <img src={logoSvg} alt="Logo" className={styles.logo} />
        </header>

        <div className={styles.content}>
          <h1 className={styles.heroTitle}>{title}</h1>
          <p className={styles.description}>{description}</p>

          <div className={styles.ctaSection}>
            <p className={styles.smallTitle}>{smallTitle}</p>
            <Button
              variant="secondary"
              size="default"
              className={styles.searchButton}
              onClick={onCtaClick}
            >
              <Search />
              {ctaText}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Hero;
