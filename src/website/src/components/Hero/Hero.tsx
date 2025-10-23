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
  mediaType?: "image" | "video";
  mediaSrc?: string;
  mediaAlt?: string;
}

// Helper function to render text with line breaks
const renderTextWithLineBreaks = (text: string) => {
  return text.split("\n").map((line, index, array) => (
    <React.Fragment key={index}>
      {line}
      {index < array.length - 1 && <br />}
    </React.Fragment>
  ));
};

const Hero: React.FC<HeroProps> = ({
  title = "Hero title",
  description = "Description",
  h2Title = "Success Stories",
  smallTitle = "Small title",
  ctaText = "Search",
  onCtaClick,
  mediaType,
  mediaSrc,
  mediaAlt = "Demo",
}) => {
  return (
    <div className={styles.hero}>
      <div className={styles.container}>
        <header className={styles.header}>
          <img src={logoSvg} alt="Logo" className={styles.logo} />
        </header>

        <div className={styles.content}>
          <div className={styles.textContent}>
            <h1 className={styles.heroTitle}>{renderTextWithLineBreaks(title)}</h1>
            <p className={styles.description}>{renderTextWithLineBreaks(description)}</p>

            <Card variant="cta" padding="medium" className={styles.ctaSection}>
              <div className={styles.h2TitleContainer}>
                <h2 className={styles.h2Title}>{renderTextWithLineBreaks(h2Title)}</h2>
                <p className={styles.smallTitle}>{renderTextWithLineBreaks(smallTitle)}</p>
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

          {mediaSrc && (
            <div className={styles.mediaContent}>
              {mediaType === "video" ? (
                <video
                  className={styles.media}
                  controls
                  autoPlay
                  loop
                  muted
                  playsInline
                >
                  <source src={mediaSrc} type="video/mp4" />
                  Your browser does not support the video tag.
                </video>
              ) : (
                <img
                  src={mediaSrc}
                  alt={mediaAlt}
                  className={styles.media}
                />
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Hero;
