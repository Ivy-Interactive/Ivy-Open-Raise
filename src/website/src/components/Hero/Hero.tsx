import React from "react";
import styles from "./Hero.module.scss";
import Card from "../Card/Card";
import { SquareArrowOutUpRight } from "lucide-react";
import { Button } from "../ui/button";
import logoSvg from "@/assets/logo/open-raise-logo-white.svg";

interface HeroProps {
  title?: string;
  description?: string;
  h2Title?: string;
  smallTitle?: string;
  ctaText?: string;
  onCtaClick?: () => void;
}

const Hero: React.FC<HeroProps> = ({
  title = "Hero title",
  description = "Description",
  h2Title = "Success Stories",
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

          <Card variant="cta" padding="medium" className={styles.ctaSection}>
            <div className={styles.h2TitleContainer}>
              <h2 className={styles.h2Title}>{h2Title}</h2>
              <p className={styles.smallTitle}>{smallTitle}</p>
            </div>
            <Button
              variant="default"
              size="default"
              className={styles.searchButton}
              onClick={onCtaClick}
            >
              <SquareArrowOutUpRight />
              {ctaText}
            </Button>
          </Card>
        </div>
      </div>
    </div>
  );
};

export default Hero;
