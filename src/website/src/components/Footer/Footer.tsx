import React from "react";
import styles from "./Footer.module.scss";
import logoSvg from "@/assets/logo/open-raise-logo-white.svg";

interface FooterLink {
  label: string;
  href: string;
}

interface FooterSection {
  title: string;
  links: FooterLink[];
}

interface FooterProps {
  sections?: FooterSection[];
  socialLinks?: {
    twitter?: string;
    linkedin?: string;
    github?: string;
  };
  copyrightText?: string;
}

const Footer: React.FC<FooterProps> = ({
  sections,
  socialLinks,
  copyrightText = `© ${new Date().getFullYear()} OpenRaise by Ivy. All rights reserved.`,
}) => {
  const defaultSections: FooterSection[] = [
    {
      title: "Product",
      links: [
        { label: "Home", href: "https://ivy.app/" },
        { label: "Pricing", href: "https://ivy.app/pricing" },
        { label: "FAQ", href: "https://ivy.app/faq" },
        { label: "Contact", href: "https://ivy.app/contact" },
      ],
    },
    {
      title: "Careers",
      links: [
        { label: "Career Opportunities", href: "https://ivy.app/career" },
        { label: "Engineering Manager", href: "https://ivy.app/career/founding-engineering-manager" },
        { label: "AI Engineer", href: "https://ivy.app/career/founding-ai-engineer" },
        { label: "Design Engineer", href: "https://ivy.app/career/founding-design-engineer-1" },
        { label: "Community Manager", href: "https://ivy.app/career/open-source-community-manager" },
        { label: "Growth Specialist", href: "https://ivy.app/career/product-led-growth-specialist" },
      ],
    },
    {
      title: "Resources",
      links: [
        { label: "AI-First Framework for .NET", href: "https://ivy.app/blog/the-ai-first-framework-for-net-business-apps" },
        { label: "AI in Dashboard Development", href: "https://ivy.app/blog/how-ai-is-disrupting-dashboard-development-in-net-with-ivy" },
        { label: "Becoming AI-First", href: "https://ivy.app/blog/transforming-your-company-into-an-ai-first-company" },
        { label: "Business Opportunities", href: "https://ivy.app/blog/the-ai-first-shift-business-opportunities-with-ivy-s-framework" },
        { label: "Fortnox Integration", href: "https://ivy.app/partner/automate-your-fortnox-accounting-with-ivy" },
      ],
    },
    {
      title: "Legal",
      links: [
        { label: "Terms of Service", href: "https://ivy.app/legal/terms-of-service" },
        { label: "Privacy Policy", href: "https://ivy.app/legal/privacy-policy" },
        { label: "Cookie Policy", href: "https://ivy.app/legal/cookie-policy" },
        { label: "Refund Policy", href: "https://ivy.app/legal/refund-policy" },
      ],
    },
  ];

  const displaySections = sections || defaultSections;

  return (
    <footer className={styles.footer}>
      <div className={styles.container}>
        <div className={styles.topSection}>
          <div className={styles.brandSection}>
            <img src={logoSvg} alt="OpenRaise Logo" className={styles.logo} />
            <p className={styles.tagline}>
              Connecting founders with top-tier venture capital firms to
              accelerate growth.
            </p>
          </div>

          <div className={styles.linksGrid}>
            {displaySections.map((section, index) => (
              <div key={index} className={styles.linkSection}>
                <h3 className={styles.sectionTitle}>{section.title}</h3>
                <ul className={styles.linkList}>
                  {section.links.map((link, linkIndex) => (
                    <li key={linkIndex}>
                      <a
                        href={link.href}
                        className={styles.link}
                        target="_blank"
                        rel="noopener noreferrer"
                      >
                        {link.label}
                      </a>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>

        <div className={styles.bottomSection}>
          <p className={styles.copyright}>{copyrightText}</p>
          {socialLinks && (
            <div className={styles.socialLinks}>
              {socialLinks.twitter && (
                <a
                  href={socialLinks.twitter}
                  className={styles.socialLink}
                  aria-label="Twitter"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <svg
                    width="20"
                    height="20"
                    viewBox="0 0 24 24"
                    fill="currentColor"
                  >
                    <path d="M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z" />
                  </svg>
                </a>
              )}
              {socialLinks.linkedin && (
                <a
                  href={socialLinks.linkedin}
                  className={styles.socialLink}
                  aria-label="LinkedIn"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <svg
                    width="20"
                    height="20"
                    viewBox="0 0 24 24"
                    fill="currentColor"
                  >
                    <path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z" />
                  </svg>
                </a>
              )}
              {socialLinks.github && (
                <a
                  href={socialLinks.github}
                  className={styles.socialLink}
                  aria-label="GitHub"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <svg
                    width="20"
                    height="20"
                    viewBox="0 0 24 24"
                    fill="currentColor"
                  >
                    <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
                  </svg>
                </a>
              )}
            </div>
          )}
        </div>
      </div>
    </footer>
  );
};

export default Footer;
