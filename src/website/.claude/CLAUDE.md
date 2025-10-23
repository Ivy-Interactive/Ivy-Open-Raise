# OpenRaise - Claude Code Project Instructions

## Project Overview
OpenRaise is an open-source venture capital fundraising platform built by Ivy. The platform aims to democratize access to venture capital by connecting startups with investors through a transparent, fair, and community-driven approach.

## Tech Stack
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite
- **Styling**: Tailwind CSS
- **UI Components**: shadcn/ui
- **Icons**: Lucide React
- **Font**: Inter (loaded from rsms.me)

## Project Structure
```
src/website/
├── .claude/              # Claude Code configuration
├── public/              # Static assets
│   └── open-raise-logo-white.svg
├── src/
│   ├── components/      # React components
│   │   ├── ui/         # shadcn/ui components
│   │   └── Hero.tsx    # Hero section component
│   ├── lib/            # Utility functions
│   ├── App.tsx         # Main app component
│   └── main.tsx        # Entry point
├── index.html          # HTML template
├── package.json        # Dependencies
├── tailwind.config.js  # Tailwind configuration
├── tsconfig.json       # TypeScript configuration
└── vite.config.ts      # Vite configuration
```

## Development Guidelines

### Component Development
1. Use TypeScript for all components
2. Follow React best practices and hooks patterns
3. Use shadcn/ui components as base UI primitives
4. Maintain consistent styling with Tailwind CSS
5. Keep components modular and reusable

### Styling Guidelines
- **Primary Background**: `#041209` (dark green)
- **Accent Color**: `#00CC92` (teal)
- **Typography**: Inter font family
- **Responsive Design**: Mobile-first approach with breakpoints:
  - sm: 640px
  - md: 768px
  - lg: 1024px
  - xl: 1280px
  - 2xl: 1536px

### Code Style
- Use functional components with TypeScript
- Implement proper error handling
- Write clean, self-documenting code
- Use meaningful variable and function names
- Follow ESLint and Prettier configurations

### Git Workflow
1. Create feature branches from main
2. Make atomic commits with clear messages
3. Test thoroughly before pushing
4. Create pull requests for code review

## Key Features to Implement

### Phase 1 - Landing Page
- [x] Hero section with value proposition
- [ ] Features section highlighting platform benefits
- [ ] How it works section
- [ ] Testimonials/Social proof
- [ ] Footer with navigation

### Phase 2 - Core Platform
- [ ] User authentication (founders & investors)
- [ ] Startup profiles
- [ ] Investor dashboards
- [ ] Pitch deck upload/viewer
- [ ] Funding round management

### Phase 3 - Advanced Features
- [ ] Deal flow management
- [ ] Due diligence tools
- [ ] Cap table management
- [ ] Legal document templates
- [ ] Communication tools

## Important Notes

### Figma Integration
- Design files are available in Figma
- Use the MCP Figma tools to fetch latest designs
- Maintain consistency with Figma designs

### Security Considerations
- Never expose sensitive API keys in code
- Use environment variables for configuration
- Implement proper authentication and authorization
- Follow OWASP security best practices

### Performance Optimization
- Lazy load components where appropriate
- Optimize images and assets
- Implement code splitting
- Monitor bundle size

## Available Scripts
```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run linting
npm run lint

# Format code
npm run format
```

## Environment Variables
Create a `.env` file in the root directory:
```
VITE_API_URL=your_api_url
VITE_APP_NAME=OpenRaise
```

## Support & Documentation
- GitHub Issues for bug reports
- Documentation in `/docs` folder
- Follow semantic versioning for releases

## Claude Code Specific Instructions

### When Working on UI Components:
1. Always check Figma for latest designs using MCP tools
2. Use shadcn/ui components as foundation
3. Ensure responsive design across all breakpoints
4. Test with different content lengths

### When Adding New Features:
1. Start with understanding requirements
2. Plan the implementation approach
3. Create modular, reusable components
4. Write clean, maintainable code
5. Test thoroughly before marking complete

### When Debugging:
1. Check browser console for errors
2. Verify all imports are correct
3. Ensure environment variables are set
4. Check network requests in browser DevTools

## Contact
- Platform: OpenRaise by Ivy
- Website: [To be deployed]
- Repository: [GitHub link]