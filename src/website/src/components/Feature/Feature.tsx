import React from "react";
import styles from "./Feature.module.scss";
import Card from "../Card/Card";
import { Zap, Shield, Rocket, BarChart3, Users, Layers } from "lucide-react";

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
    <Card variant="hover" padding="large" className={styles.featureCard}>
      <div className={styles.iconWrapper}>{icon}</div>
      <h3 className={styles.featureTitle}>{title}</h3>
      <p className={styles.featureDescription}>{description}</p>
    </Card>
  );
};

interface FeatureProps {
  features?: FeatureCardProps[];
}

const Feature: React.FC<FeatureProps> = ({ features }) => {
  const defaultFeatures: FeatureCardProps[] = [
    {
      icon: <BarChart3 size={28} />,
      title: "Real-time Analytics",
      description:
        "Track your fundraising metrics in real-time. Monitor deal progress, investor engagement, and pipeline performance.",
    },
    {
      icon: <Users size={28} />,
      title: "Investor CRM",
      description:
        "Manage all investor relationships in one place. Track interactions, schedule follow-ups, and nurture relationships.",
    },
    {
      icon: <Layers size={28} />,
      title: "Pipeline Management",
      description:
        "Visualize your entire fundraising pipeline. Move deals through stages and never lose track of opportunities.",
    },
    {
      icon: <Zap size={28} />,
      title: "Fast Fundraising",
      description:
        "Streamline your process from pitch to close. Automate repetitive tasks and focus on what matters.",
    },
    {
      icon: <Shield size={28} />,
      title: "Secure Platform",
      description:
        "Enterprise-grade security for your sensitive data. End-to-end encryption and compliance built-in.",
    },
    {
      icon: <Rocket size={28} />,
      title: "Scale Your Growth",
      description:
        "Access tools and insights that grow with you. From seed to Series C and beyond.",
    },
  ];

  const displayFeatures = features || defaultFeatures;

  return (
    <section className={styles.feature} id="features">
      <div className={styles.container}>
        <div className={styles.header}>
          <div className={styles.badge}>
            <span className={styles.badgeIcon}>⚡</span>
            <span className={styles.badgeText}>Powerful Features</span>
          </div>
          <h2 className={styles.sectionTitle}>
            Powerful Features to
            <br />
            Supercharge Your Fundraise
          </h2>
          <p className={styles.sectionSubtitle}>
            Everything you need to track, automate, and scale your fundraising
            process, designed to boost performance and help your team close
            deals faster.
          </p>
        </div>
        <div className={styles.featuresGrid}>
          {displayFeatures.map((feature, index) => (
            <FeatureCard key={index} {...feature} />
          ))}
        </div>
      </div>
    </section>
  );
};

export default Feature;
