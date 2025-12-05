import React from "react";
import styles from "./Feature.module.scss";
import {
  Kanban,
  User,
  Link,
  Settings,
  Server,
  Sparkles,
  Code,
} from "lucide-react";

interface FeatureCardProps {
  icon: React.ReactNode;
  title: string;
  description: string;
}

const FeatureCard: React.FC<FeatureCardProps> = ({
  icon,
  title,
  description,
}) => {
  return (
    <div className={styles.featureCard}>
      <div className={styles.iconWrapper}>{icon}</div>
      <h3 className={styles.featureTitle}>{title}</h3>
      <p className={styles.featureDescription}>{description}</p>
    </div>
  );
};

const features: FeatureCardProps[] = [
  {
    icon: <Kanban size={28} />,
    title: "Investor Pipeline",
    description:
      "A simple, editable view of every investor and stage of the raise.",
  },
  {
    icon: <User size={28} />,
    title: "Investor Profiles & Timeline",
    description: "All context in one place — not scattered across tools.",
  },
  {
    icon: <Link size={28} />,
    title: "Deck Tracking Links",
    description:
      "See who viewed your deck, for how long, and which slides they returned to.",
  },
  {
    icon: <Settings size={28} />,
    title: "Customizable Logic",
    description: "Adapt fields, stages, and workflows without engineering.",
  },
  {
    icon: <Server size={28} />,
    title: "Self-host or One-Click Deploy",
    description: "Run on your own infrastructure or deploy in minutes.",
  },
  {
    icon: <Sparkles size={28} />,
    title: "Magic-Link Login",
    description: "No passwords. Simple, secure access.",
  },
  {
    icon: <Code size={28} />,
    title: "Open Source",
    description:
      "No licenses. No constraints. Full freedom to modify and extend.",
  },
];

const Feature: React.FC = () => {
  return (
    <section className={styles.feature} id="features">
      <div className={styles.container}>
        <div className={styles.header}>
          <div className={styles.badge}>
            <span className={styles.badgeText}>POWERFUL FEATURES</span>
          </div>
          <h2 className={styles.sectionTitle}>
            Everything you need to
            <br />
            manage your fundraise
          </h2>
        </div>
        <div className={styles.featuresGrid}>
          {features.map((feature, index) => (
            <FeatureCard key={index} {...feature} />
          ))}
        </div>
      </div>
    </section>
  );
};

export default Feature;
