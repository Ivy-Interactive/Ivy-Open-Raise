import Navbar from "./components/Navbar/Navbar";
import Hero from "./components/Hero/Hero";
import AboutSection from "./components/AboutSection/AboutSection";
import ProductFeatures from "./components/ProductFeatures/ProductFeatures";
import Feature from "./components/Feature/Feature";
import CalendlySection from "./components/CalendlySection/CalendlySection";
import Footer from "./components/Footer/Footer";

import "./App.css";

function App() {
  return (
    <div className="App">
      <Navbar />
      <Hero
        badge="Explore Our New AI Features"
        title={`Smarter Fundraising for\nFaster Startup Growth`}
        description="Track your fundraising performance in real-time, manage investor pipelines effortlessly, and gain insights that help you close deals faster."
        primaryCtaText="View on GitHub"
        secondaryCtaText="Book a Demo"
        githubUrl="https://github.com/Ivy-Interactive/Ivy-Open-Raise"
      />
      <AboutSection
        title="Our Platform is designed to give teams clear insights, real-time tracking, and powerful tools to close deals faster."
        stats={[
          { value: "25M+", label: "Active Users" },
          { value: "$50B+", label: "Revenue" },
          { value: "300%", label: "Faster Sales" },
          { value: "500K+", label: "Deals Closed" },
        ]}
      />
      <ProductFeatures />
      <Feature />
      <CalendlySection
        url="https://calendly.com/d/cv34-53t-7q5/ivy-demo-call-with-our-founding-growth-team?hide_gdpr_banner=1&background_color=ffffff&text_color=000000&primary_color=00cc92"
        title="Book a Demo"
        subtitle="Schedule a call with our team to see OpenRaise in action"
      />
      <Footer />
    </div>
  );
}

export default App;
