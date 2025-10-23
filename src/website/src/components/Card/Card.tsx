import React from "react";
import styles from "./Card.module.scss";

interface CardProps {
  children: React.ReactNode;
  className?: string;
  variant?: "default" | "hover" | "cta";
  padding?: "small" | "medium" | "large";
  onClick?: () => void;
}

const Card: React.FC<CardProps> = ({
  children,
  className = "",
  variant = "default",
  padding = "medium",
  onClick,
}) => {
  const cardClasses = [
    styles.card,
    styles[`card--${variant}`],
    styles[`card--padding-${padding}`],
    className,
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={cardClasses} onClick={onClick}>
      {children}
    </div>
  );
};

export default Card;